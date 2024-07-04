csharp
public partial class StockPickingBatch 
{
    public void _ComputeShowLotsText()
    {
        if (Env.Get<StockPicking>().Search(x => x.Id == this.PickingIds[0].Id).ShowLotsText)
        {
            this.ShowLotsText = true;
        }
    }

    public void _ComputeAllowedPickingIds()
    {
        var allowedPickingStates = new List<string>() { "Waiting", "Confirmed", "Assigned" };

        if (this.State == "Draft")
        {
            allowedPickingStates.Add("Draft");
        }

        var pickingIds = Env.Get<StockPicking>().Search(x => x.CompanyId == this.CompanyId && allowedPickingStates.Contains(x.State));
        if (this.PickingTypeId != null)
        {
            pickingIds = pickingIds.Where(x => x.PickingTypeId == this.PickingTypeId).ToList();
        }
        this.AllowedPickingIds = pickingIds;
    }

    public void _ComputeMoveIds()
    {
        this.MoveIds = Env.Get<StockMove>().Search(x => x.PickingId.Id.IsIn(this.PickingIds.Select(x => x.Id)));
        this.MoveLineIds = Env.Get<StockMoveLine>().Search(x => x.PickingId.Id.IsIn(this.PickingIds.Select(x => x.Id)));
        this.ShowCheckAvailability = this.MoveIds.Any(x => x.State != "Assigned" && x.State != "Cancel" && x.State != "Done");
    }

    public void _ComputeShowAllocation()
    {
        if (!Env.User.IsInGroup("stock.group_reception_report"))
        {
            return;
        }

        this.ShowAllocation = this.PickingIds.Any(x => x._GetShowAllocation(this.PickingTypeId));
    }

    public void _ComputeState()
    {
        if (this.State == "Cancel" || this.State == "Done")
        {
            return;
        }

        if (!this.PickingIds.Any())
        {
            return;
        }

        if (this.PickingIds.All(x => x.State == "Cancel"))
        {
            this.State = "Cancel";
        }
        else if (this.PickingIds.All(x => x.State == "Cancel" || x.State == "Done"))
        {
            this.State = "Done";
        }
    }

    public void _ComputeScheduledDate()
    {
        if (this.PickingIds.Any(x => x.ScheduledDate != null))
        {
            this.ScheduledDate = this.PickingIds.Where(x => x.ScheduledDate != null).Min(x => x.ScheduledDate);
        }
    }

    public void OnchangeScheduledDate()
    {
        if (this.ScheduledDate != null)
        {
            Env.Get<StockPicking>().Search(x => x.Id.IsIn(this.PickingIds.Select(x => x.Id))).UpdateAll(x => x.ScheduledDate = this.ScheduledDate);
        }
    }

    public void _SetMoveLineIds()
    {
        var moveLines = this.MoveLineIds.Where(x => x.PickingId.Id == this.PickingIds[0].Id).ToList();
        foreach (var picking in this.PickingIds)
        {
            picking.MoveLineIds = moveLines.Where(x => x.PickingId.Id == picking.Id).ToList();
        }
    }

    public void ActionConfirm()
    {
        if (!this.PickingIds.Any())
        {
            throw new Exception("You have to set some pickings to batch.");
        }

        Env.Get<StockPicking>().Search(x => x.Id.IsIn(this.PickingIds.Select(x => x.Id))).ActionConfirm();
        this._CheckCompany();
        this.State = "InProgress";
    }

    public void ActionCancel()
    {
        this.State = "Cancel";
        this.PickingIds = new List<StockPicking>();
    }

    public void ActionPrint()
    {
        Env.Get<IrActionsReport>().Search(x => x.Name == "action_report_picking_batch").ReportAction(this);
    }

    public void ActionDone()
    {
        this._CheckCompany();
        var pickings = this.PickingIds.Where(x => x.State != "Cancel" && x.State != "Done").ToList();
        var emptyPickings = this.PickingIds.Where(x => x.State == "Waiting" && HasNoQuantity(x) || x.State == "Assigned" && IsEmpty(x)).ToList();
        pickings = pickings.Except(emptyPickings).ToList();

        var emptyPickingsToDetach = new List<StockPicking>();
        foreach (var picking in pickings)
        {
            if (HasNoQuantity(picking))
            {
                emptyPickingsToDetach.Add(picking);
            }
            picking.MessagePost(x => x.Body = $"<b>{Env.Translate("Transferred by")}</b>: {Env.Translate("Batch Transfer")} <a href=#id={this.Id}&view_type=form&model=Stock.StockPickingBatch>{this.Name}</a>");
        }

        var context = new Dictionary<string, object>() { { "skip_sanity_check", true }, { "pickings_to_detach", emptyPickingsToDetach.Select(x => x.Id).ToList() } };
        if (emptyPickingsToDetach.Count == pickings.Count)
        {
            Env.Get<StockPicking>().Search(x => x.Id.IsIn(pickings.Select(x => x.Id))).WithContext(context).ButtonValidate();
        }
        else
        {
            pickings = pickings.Except(emptyPickingsToDetach).ToList();
            context["pickings_to_detach"] = context["pickings_to_detach"] as List<int>;
            context["pickings_to_detach"].AddRange(emptyPickingsToDetach.Select(x => x.Id).ToList());
            Env.Get<StockPicking>().Search(x => x.Id.IsIn(pickings.Select(x => x.Id))).WithContext(context).ButtonValidate();
        }
    }

    public void ActionAssign()
    {
        Env.Get<StockPicking>().Search(x => x.Id.IsIn(this.PickingIds.Select(x => x.Id))).ActionAssign();
    }

    public void ActionPutInPack()
    {
        if (this.State != "Done" && this.State != "Cancel")
        {
            var moveLineIds = this.PickingIds[0]._PackageMoveLines(true);
            if (moveLineIds.Any())
            {
                var res = this.PickingIds[0]._PrePutInPackHook(moveLineIds);
                if (res != null)
                {
                    return;
                }

                var package = this.PickingIds[0]._PutInPack(moveLineIds);
                this.PickingIds[0]._PostPutInPackHook(package);
                return;
            }

            throw new Exception(Env.Translate("Please add 'Done' quantities to the batch picking to create a new pack."));
        }
    }

    public void ActionViewReceptionReport()
    {
        var action = this.PickingIds[0].ActionViewReceptionReport();
        action.Context["default_picking_ids"] = this.PickingIds.Select(x => x.Id).ToList();
        return action;
    }

    public void ActionOpenLabelLayout()
    {
        if (Env.User.IsInGroup("stock.group_production_lot") && this.MoveLineIds.Any(x => x.LotId != null))
        {
            var view = Env.Get<IrActionsActWindow>().Search(x => x.Name == "picking_label_type_form");
            return new IrActionsActWindow()
            {
                Name = Env.Translate("Choose Type of Labels To Print"),
                Type = "ir.actions.act_window",
                ResModel = "picking.label.type",
                Views = new List<object>() { new object[] { view.Id, "form" } },
                Target = "new",
                Context = new Dictionary<string, object>() { { "default_picking_ids", this.PickingIds.Select(x => x.Id).ToList() } }
            };
        }

        var view = Env.Get<IrActionsActWindow>().Search(x => x.Name == "product_label_layout_form_picking");
        return new IrActionsActWindow()
        {
            Name = Env.Translate("Choose Labels Layout"),
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "product.label.layout",
            Views = new List<object>() { new object[] { view.Id, "form" } },
            ViewId = view.Id,
            Target = "new",
            Context = new Dictionary<string, object>()
            {
                { "default_product_ids", this.MoveLineIds.Select(x => x.ProductId.Id).ToList() },
                { "default_move_ids", this.MoveIds.Select(x => x.Id).ToList() },
                { "default_move_quantity", "move" }
            }
        };
    }

    public void _SanityCheck()
    {
        if (!this.PickingIds.All(x => x.Id.IsIn(this.AllowedPickingIds.Select(x => x.Id))))
        {
            var erroneousPickings = this.PickingIds.Except(this.AllowedPickingIds).ToList();
            throw new Exception(Env.Translate("The following transfers cannot be added to batch transfer {0}. Please check their states and operation types.\n\nIncompatibilities: {1}", this.Name, Env.Get<StockPicking>().Search(x => x.Id.IsIn(erroneousPickings.Select(x => x.Id))).Select(x => x.Name).Aggregate((a, b) => a + ", " + b)));
        }
    }

    public void _TrackSubtype(Dictionary<string, object> initValues)
    {
        if (initValues.ContainsKey("State"))
        {
            var mtBatchState = Env.Get<IrModel>().Search(x => x.Name == "mt_batch_state");
            // ... 
        }
        else
        {
            // ...
        }
    }

    public bool _IsPickingAutoMergeable(StockPicking picking)
    {
        var res = true;
        if (this.PickingTypeId.BatchMaxLines > 0)
        {
            res = res && (this.MoveIds.Count + picking.MoveIds.Count <= this.PickingTypeId.BatchMaxLines);
        }
        if (this.PickingTypeId.BatchMaxPickings > 0)
        {
            res = res && (this.PickingIds.Count + 1 <= this.PickingTypeId.BatchMaxPickings);
        }
        return res;
    }

    // Private Helper Methods
    private bool HasNoQuantity(StockPicking picking)
    {
        return picking.MoveIds.Where(x => x.State != "Done" && x.State != "Cancel").All(x => !x.Picked || Env.Get<ProductUom>().Search(x => x.Id == x.ProductUom.Id).IsZero(x.Quantity));
    }

    private bool IsEmpty(StockPicking picking)
    {
        return picking.MoveIds.Where(x => x.State != "Done" && x.State != "Cancel").All(x => Env.Get<ProductUom>().Search(x => x.Id == x.ProductUom.Id).IsZero(x.Quantity));
    }

    private void _CheckCompany()
    {
        // ... implementation
    }
}
