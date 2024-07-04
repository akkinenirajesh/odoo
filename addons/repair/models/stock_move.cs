C#
public partial class RepairStockMove
{
    public virtual void ComputeForecastInformation()
    {
        if (this.RepairLineType == null || this.RepairLineType == "Add")
        {
            return;
        }

        this.ForecastAvailability = this.ProductQty;
        this.ForecastExpectedDate = null;
    }

    public virtual void ComputePickingTypeId()
    {
        if (this.RepairId != null)
        {
            this.PickingTypeId = this.RepairId.PickingTypeId;
            return;
        }
    }

    public virtual void ComputeLocationDestId()
    {
        if (this.RepairId != null && this.RepairLineType != null)
        {
            switch (this.RepairLineType)
            {
                case "Add":
                    this.LocationDestId = this.RepairId.LocationDestId;
                    break;
                case "Remove":
                    this.LocationDestId = this.RepairId.PartsLocationId;
                    break;
                case "Recycle":
                    this.LocationDestId = this.RepairId.RecycleLocationId;
                    break;
            }
        }
    }

    public virtual List<object> CopyData(object defaultValues)
    {
        var valsList = Env.Call("copy_data", this, defaultValues);
        for (int i = 0; i < valsList.Count; i++)
        {
            var vals = valsList[i] as Dictionary<string, object>;
            if (defaultValues.ContainsKey("RepairId") || this.RepairId != null)
            {
                vals["SaleLineId"] = null;
            }
        }
        return valsList;
    }

    public virtual void UnlinkIfDraftOrCancel()
    {
        if (this.RepairId != null)
        {
            this.ActionCancel();
        }
        Env.Call("unlink_if_draft_or_cancel", this);
    }

    public virtual void Unlink()
    {
        this.CleanRepairSaleOrderLine();
        Env.Call("unlink", this);
    }

    public virtual void Create(object vals)
    {
        if (vals.ContainsKey("RepairId") && vals.ContainsKey("RepairLineType"))
        {
            var repairId = Env.Ref<RepairOrder>(vals["RepairId"]);
            vals["Name"] = repairId.Name;
            var (srcLocation, destLocation) = this.GetRepairLocations(vals["RepairLineType"], repairId);
            if (!vals.ContainsKey("LocationId"))
            {
                vals["LocationId"] = srcLocation.Id;
            }
            if (!vals.ContainsKey("LocationDestId"))
            {
                vals["LocationDestId"] = destLocation.Id;
            }
        }
        var moves = Env.Call("create", this, new List<object> { vals });
        var repairMoves = Env.Ref<RepairStockMove>();
        foreach (var move in moves)
        {
            if (move.RepairId == null)
            {
                continue;
            }
            move.GroupId = move.RepairId.ProcurementGroupId.Id;
            move.Origin = move.Name;
            move.PickingTypeId = move.RepairId.PickingTypeId.Id;
            repairMoves = repairMoves.Concat(new List<RepairStockMove> { move });
            if (move.State == "draft" && move.RepairId.State in new List<string> { "confirmed", "under_repair" })
            {
                move.CheckCompany();
                move.AdjustProcureMethod();
                move.ActionConfirm();
                move.TriggerScheduler();
            }
        }
        repairMoves.CreateRepairSaleOrderLine();
    }

    public virtual void Write(object vals)
    {
        Env.Call("write", this, vals);
        var repairMoves = Env.Ref<RepairStockMove>();
        var movesToCreateSoLine = Env.Ref<RepairStockMove>();
        foreach (var move in this)
        {
            if (move.RepairId == null)
            {
                continue;
            }
            if (vals.ContainsKey("RepairLineType") || (vals.ContainsKey("PickingTypeId") && move.ProductId != move.RepairId.ProductId))
            {
                (move.LocationId, move.LocationDestId) = move.GetRepairLocations(move.RepairLineType);
            }
            if (move.SaleLineId == null && !vals.ContainsKey("SaleLineId") && move.RepairLineType == "Add")
            {
                movesToCreateSoLine = movesToCreateSoLine.Concat(new List<RepairStockMove> { move });
            }
            if (move.SaleLineId != null && (vals.ContainsKey("RepairLineType") || vals.ContainsKey("ProductUomQty")))
            {
                repairMoves = repairMoves.Concat(new List<RepairStockMove> { move });
            }
        }
        repairMoves.UpdateRepairSaleOrderLine();
        movesToCreateSoLine.CreateRepairSaleOrderLine();
    }

    public virtual void ActionAddFromCatalogRepair()
    {
        var repairOrder = Env.Ref<RepairOrder>(Env.Context.Get("order_id"));
        repairOrder.ActionAddFromCatalog();
    }

    public virtual void ActionCancel()
    {
        this.CleanRepairSaleOrderLine();
        Env.Call("action_cancel", this);
    }

    public virtual void CreateRepairSaleOrderLine()
    {
        if (this == null)
        {
            return;
        }
        var soLineVals = new List<Dictionary<string, object>>();
        foreach (var move in this)
        {
            if (move.SaleLineId != null || move.RepairLineType != "Add" || move.RepairId.SaleOrderId == null)
            {
                continue;
            }
            var productQty = move.RepairId.State != "done" ? move.ProductUomQty : move.Quantity;
            soLineVals.Add(new Dictionary<string, object>()
            {
                {"OrderId", move.RepairId.SaleOrderId.Id},
                {"ProductId", move.ProductId.Id},
                {"ProductUomQty", productQty},
                {"MoveIds", new List<object> { Env.Link(move) }},
            });
            if (move.RepairId.UnderWarranty)
            {
                soLineVals.Last()["PriceUnit"] = 0.0;
            }
            else if (move.PriceUnit != null)
            {
                soLineVals.Last()["PriceUnit"] = move.PriceUnit;
            }
        }
        Env.Ref<SaleOrderLine>().Create(soLineVals);
    }

    public virtual void CleanRepairSaleOrderLine()
    {
        foreach (var m in this.Where(m => m.RepairId != null && m.SaleLineId != null))
        {
            m.SaleLineId.Write(new Dictionary<string, object> { { "ProductUomQty", 0.0 } });
        }
    }

    public virtual void UpdateRepairSaleOrderLine()
    {
        if (this == null)
        {
            return;
        }
        var movesToClean = Env.Ref<RepairStockMove>();
        var movesToUpdate = Env.Ref<RepairStockMove>();
        foreach (var move in this)
        {
            if (move.RepairId == null)
            {
                continue;
            }
            if (move.SaleLineId != null && move.RepairLineType != "Add")
            {
                movesToClean = movesToClean.Concat(new List<RepairStockMove> { move });
            }
            if (move.SaleLineId != null && move.RepairLineType == "Add")
            {
                movesToUpdate = movesToUpdate.Concat(new List<RepairStockMove> { move });
            }
        }
        movesToClean.CleanRepairSaleOrderLine();
        foreach (var saleLine in movesToUpdate.GroupBy(m => m.SaleLineId).Select(g => g.Key))
        {
            saleLine.ProductUomQty = saleLine.MoveIds.Sum(m => m.ProductUomQty);
        }
    }

    public virtual bool IsConsuming()
    {
        return Env.Call("is_consuming", this) || (this.RepairId != null && this.RepairLineType == "Add");
    }

    public virtual (StockLocation, StockLocation) GetRepairLocations(object repairLineType, RepairOrder repairId = null)
    {
        if (repairId == null)
        {
            repairId = this.RepairId;
        }
        switch (repairLineType)
        {
            case "Add":
                return (repairId.LocationId, repairId.LocationDestId);
            case "Remove":
                return (repairId.LocationDestId, repairId.PartsLocationId);
            case "Recycle":
                return (repairId.LocationDestId, repairId.RecycleLocationId);
        }
        return (null, null);
    }

    public virtual object GetSourceDocument()
    {
        return this.RepairId ?? Env.Call("get_source_document", this);
    }

    public virtual void SetRepairLocations()
    {
        foreach (var moves in this.Where(m => (m.RepairId != null && m.RepairLineType != null)).GroupBy(m => m.RepairId).Select(g => g.ToList()))
        {
            foreach (var (lineType, m) in moves.GroupBy(m => m.RepairLineType).Select(g => (g.Key, g.ToList())))
            {
                (m.LocationId, m.LocationDestId) = m.GetRepairLocations(lineType);
            }
        }
    }

    public virtual bool ShouldBeAssigned()
    {
        return this.RepairId == null ? Env.Call("should_be_assigned", this) : false;
    }

    public virtual List<RepairStockMove> Split(double qty, object restrictPartnerId = null)
    {
        if (this.RepairId != null)
        {
            return new List<RepairStockMove>();
        }
        return Env.Call("split", this, qty, restrictPartnerId) as List<RepairStockMove>;
    }

    public virtual object ActionShowDetails()
    {
        var action = Env.Call("action_show_details", this);
        if (this.RepairLineType == "Recycle")
        {
            action["context"] = action["context"] as Dictionary<string, object>;
            action["context"].Add("show_quant", false);
            action["context"].Add("show_destination_location", true);
        }
        return action;
    }
}
