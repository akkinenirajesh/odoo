csharp
public partial class PurchaseStock.PurchaseOrder
{
    public void ComputePickingIds()
    {
        this.PickingIds = Env.Get<PurchaseStock.PurchaseOrderLine>(this.OrderLine).MoveIds.PickingId;
    }

    public void ComputeIncomingPickingCount()
    {
        this.IncomingPickingCount = this.PickingIds.Count;
    }

    public void ComputeEffectiveDate()
    {
        var pickings = this.PickingIds.Where(x => x.State == "done" && x.LocationDestId.Usage != "supplier" && x.DateDone != null).ToList();
        this.EffectiveDate = pickings.Any() ? pickings.Min(x => x.DateDone) : null;
    }

    public void ComputeIsShipped()
    {
        this.IsShipped = this.PickingIds.Any() && this.PickingIds.All(x => x.State == "done" || x.State == "cancel");
    }

    public void ComputeReceiptStatus()
    {
        if (!this.PickingIds.Any() || this.PickingIds.All(p => p.State == "cancel"))
        {
            this.ReceiptStatus = null;
        }
        else if (this.PickingIds.All(p => p.State == "done" || p.State == "cancel"))
        {
            this.ReceiptStatus = "full";
        }
        else if (this.PickingIds.Any(p => p.State == "done"))
        {
            this.ReceiptStatus = "partial";
        }
        else
        {
            this.ReceiptStatus = "pending";
        }
    }

    public void ComputeDestAddressId()
    {
        if (this.PickingTypeId.DefaultLocationDestId.Usage != "customer")
        {
            this.DestAddressId = null;
        }
    }

    public void OnChangeCompanyId()
    {
        var pickingType = this.PickingTypeId;
        if (!(pickingType != null && pickingType.Code == "incoming" && (pickingType.WarehouseId.CompanyId == this.CompanyId || pickingType.WarehouseId == null)))
        {
            this.PickingTypeId = Env.Get<Stock.PickingType>().Search(new[] {
                new SearchCriteria("Code", "=", "incoming"),
                new SearchCriteria("WarehouseId.CompanyId", "=", this.CompanyId.Id)
            }).FirstOrDefault();
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("OrderLine") && this.State == "purchase")
        {
            var preOrderLineQty = this.OrderLine.ToDictionary(orderLine => orderLine, orderLine => orderLine.ProductQty);
            // Do something with preOrderLineQty here
        }
        // Call base Write() method here
    }

    public void ActionAddFromCatalog()
    {
        // Call base ActionAddFromCatalog() method here
        var kanbanViewId = Env.Get<Ir.Actions.Actions>()._ForXmlId("purchase_stock.product_view_kanban_catalog_purchase_only").Id;
        // Modify action['views'][0] here
    }

    public void ButtonApprove(bool force = false)
    {
        // Call base ButtonApprove() method here
        this.CreatePicking();
    }

    public void ButtonCancel()
    {
        var orderLinesIds = new HashSet<int>();
        var pickingsToCancelIds = new HashSet<int>();
        foreach (var order in this)
        {
            // Check if some receptions have already been done
            foreach (var move in order.OrderLine.MoveIds)
            {
                if (move.State == "done")
                {
                    throw new UserError("Unable to cancel purchase order {0} as some receptions have already been done.", order.Name);
                }
            }
            // Change procure_method of moves if product is MTO
            if (order.State == "draft" || order.State == "sent" || order.State == "to approve" || order.State == "purchase")
            {
                orderLinesIds.UnionWith(order.OrderLine.Select(ol => ol.Id));
            }

            pickingsToCancelIds.UnionWith(order.PickingIds.Where(r => r.State != "cancel").Select(r => r.Id));
        }

        var orderLines = Env.Get<PurchaseStock.PurchaseOrderLine>().Browse(orderLinesIds.ToList());
        var movesToCancelIds = new HashSet<int>();
        var movesToRecomputeIds = new HashSet<int>();
        foreach (var orderLine in orderLines)
        {
            movesToCancelIds.UnionWith(orderLine.MoveIds.Select(m => m.Id));
            if (orderLine.MoveDestIds != null)
            {
                var moveDestIds = orderLine.MoveDestIds.Where(move => move.State != "done" && !move.Scrapped && move.RuleId.RouteId == move.LocationDestId.WarehouseId.ReceptionRouteId).ToList();
                var movesToUnlink = moveDestIds.Where(m => m.CreatedPurchaseLineIds.Count > 1).ToList();
                if (movesToUnlink.Any())
                {
                    movesToUnlink.ForEach(m => m.CreatedPurchaseLineIds = m.CreatedPurchaseLineIds.Where(id => id != orderLine.Id).ToList());
                }
                moveDestIds = moveDestIds.Except(movesToUnlink).ToList();
                if (orderLine.PropagateCancel)
                {
                    movesToCancelIds.UnionWith(moveDestIds.Select(m => m.Id));
                }
                else
                {
                    movesToRecomputeIds.UnionWith(moveDestIds.Select(m => m.Id));
                }
            }
        }

        if (movesToCancelIds.Any())
        {
            var movesToCancel = Env.Get<Stock.Move>().Browse(movesToCancelIds.ToList());
            movesToCancel.ActionCancel();
        }

        if (movesToRecomputeIds.Any())
        {
            var movesToRecompute = Env.Get<Stock.Move>().Browse(movesToRecomputeIds.ToList());
            movesToRecompute.ForEach(move => move.ProcureMethod = "make_to_stock");
            movesToRecompute.RecomputeState();
        }

        if (pickingsToCancelIds.Any())
        {
            var pickingsToCancel = Env.Get<Stock.Picking>().Browse(pickingsToCancelIds.ToList());
            pickingsToCancel.ActionCancel();
        }

        if (orderLines != null)
        {
            orderLines.ForEach(ol => ol.MoveDestIds = new List<Stock.Move>());
        }

        // Call base ButtonCancel() method here
    }

    public void ActionViewPicking()
    {
        this.GetActionViewPicking(this.PickingIds);
    }

    public void GetActionViewPicking(List<Stock.Picking> pickings)
    {
        // ... Implement logic to get the action view for pickings
    }

    public Dictionary<string, object> PrepareInvoice()
    {
        var invoiceVals = (Dictionary<string, object>)base.PrepareInvoice();
        invoiceVals["InvoiceIncotermId"] = this.IncotermId.Id;
        return invoiceVals;
    }

    public void LogDecreaseOrderedQuantity(Dictionary<PurchaseStock.PurchaseOrderLine, Tuple<decimal, decimal>> purchaseOrderLinesQuantities)
    {
        // ... Implement logic to log the decrease in ordered quantity
    }

    public int GetDestinationLocation()
    {
        if (this.DestAddressId != null)
        {
            return this.DestAddressId.PropertyStockCustomer.Id;
        }
        return this.PickingTypeId.DefaultLocationDestId.Id;
    }

    public Stock.PickingType GetPickingType(int companyId)
    {
        return Env.Get<Stock.PickingType>().Search(new[] {
            new SearchCriteria("Code", "=", "incoming"),
            new SearchCriteria("WarehouseId.CompanyId", "=", companyId)
        }).FirstOrDefault() ?? Env.Get<Stock.PickingType>().Search(new[] {
            new SearchCriteria("Code", "=", "incoming"),
            new SearchCriteria("WarehouseId", "=", null)
        }).FirstOrDefault();
    }

    public Dictionary<string, object> PreparePicking()
    {
        if (this.GroupId == null)
        {
            this.GroupId = Env.Get<Procurement.Group>().Create(new Dictionary<string, object> {
                { "Name", this.Name },
                { "PartnerId", this.PartnerId.Id }
            });
        }
        if (this.PartnerId.PropertyStockSupplier.Id == null)
        {
            throw new UserError("You must set a Vendor Location for this partner {0}", this.PartnerId.Name);
        }
        return new Dictionary<string, object> {
            { "PickingTypeId", this.PickingTypeId.Id },
            { "PartnerId", this.PartnerId.Id },
            { "UserId", null },
            { "Date", this.DateOrder },
            { "Origin", this.Name },
            { "LocationDestId", this.GetDestinationLocation() },
            { "LocationId", this.PartnerId.PropertyStockSupplier.Id },
            { "CompanyId", this.CompanyId.Id },
            { "State", "draft" }
        };
    }

    public void CreatePicking()
    {
        var stockPicking = Env.Get<Stock.Picking>();
        foreach (var order in this.Where(po => po.State == "purchase" || po.State == "done"))
        {
            if (order.OrderLine.Any(product => product.Type == "consu"))
            {
                order = order.WithCompany(order.CompanyId);
                var pickings = order.PickingIds.Where(x => x.State != "done" && x.State != "cancel").ToList();
                Stock.Picking picking;
                if (!pickings.Any())
                {
                    var res = order.PreparePicking();
                    picking = stockPicking.WithUser(Env.SUPERUSER_ID).Create(res);
                    pickings = new List<Stock.Picking> { picking };
                }
                else
                {
                    picking = pickings[0];
                }
                var moves = order.OrderLine.CreateStockMoves(picking);
                moves = moves.Where(x => x.State != "done" && x.State != "cancel").ActionConfirm();
                int seq = 0;
                foreach (var move in moves.OrderBy(move => move.Date))
                {
                    seq += 5;
                    move.Sequence = seq;
                }
                moves.ActionAssign();
                var forwardPickings = Env.Get<Stock.Picking>()._GetImpactedPickings(moves);
                (pickings.Concat(forwardPickings)).ActionConfirm();
                picking.MessagePostWithSource("mail.message_origin_link", new Dictionary<string, object> {
                    { "self", picking },
                    { "origin", order }
                }, "mail.mt_note");
            }
        }
    }

    public void AddPickingInfo(Ir.Activities.Activity activity)
    {
        var validatedPicking = this.PickingIds.Where(p => p.State == "done").FirstOrDefault();
        if (validatedPicking != null)
        {
            activity.Note += "<p>Those dates couldn’t be modified accordingly on the receipt {0} which had already been validated.</p>".Format(validatedPicking.Name);
        }
        else if (!this.PickingIds.Any())
        {
            activity.Note += "<p>Corresponding receipt not found.</p>";
        }
        else
        {
            activity.Note += "<p>Those dates have been updated accordingly on the receipt {0}.</p>".Format(this.PickingIds[0].Name);
        }
    }

    public void CreateUpdateActivity(Dictionary<PurchaseStock.PurchaseOrderLine, DateTime> updatedDates)
    {
        // Call base CreateUpdateDateActivity() method here
        this.AddPickingInfo(activity);
    }

    public void UpdateUpdateActivity(Dictionary<PurchaseStock.PurchaseOrderLine, DateTime> updatedDates, Ir.Activities.Activity activity)
    {
        // Remove old picking info from activity.Note
        // Call base UpdateUpdateDateActivity() method here
        this.AddPickingInfo(activity);
    }

    public List<PurchaseStock.PurchaseOrder> GetOrdersToRemind()
    {
        // Call base GetOrdersToRemind() method here
        return base.GetOrdersToRemind().Where(p => p.EffectiveDate == null).ToList();
    }
}
