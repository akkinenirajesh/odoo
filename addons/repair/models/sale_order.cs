csharp
public partial class Repair.SaleOrder
{
    public int RepairCount { get; set; }
    public List<Repair.RepairOrder> RepairOrderIds { get; set; }

    public void ComputeRepairCount()
    {
        this.RepairCount = this.RepairOrderIds.Count;
    }

    public void ActionCancel()
    {
        var res = Env.CallMethod("super", "_action_cancel");
        this.OrderLine._CancelRepairOrder();
    }

    public void ActionConfirm()
    {
        var res = Env.CallMethod("super", "_action_confirm");
        this.OrderLine._CreateRepairOrder();
    }

    public void ActionShowRepair()
    {
        if (this.RepairCount == 1)
        {
            var action = new Dictionary<string, object>
            {
                { "type", "ir.actions.act_window" },
                { "res_model", "Repair.RepairOrder" },
                { "views", new List<List<object>> { new List<object> { false, "form" } } },
                { "res_id", this.RepairOrderIds[0].Id },
            };
            Env.ExecuteAction(action);
        }
        else if (this.RepairCount > 1)
        {
            var action = new Dictionary<string, object>
            {
                { "name", "Repair Orders" },
                { "type", "ir.actions.act_window" },
                { "res_model", "Repair.RepairOrder" },
                { "view_mode", "tree,form" },
                { "domain", new List<List<object>> { new List<object> { "sale_order_id", "=", this.Id } } },
            };
            Env.ExecuteAction(action);
        }
    }
}

public partial class Repair.SaleOrderLine
{
    public void ComputeQtyDelivered()
    {
        var remainingSOLines = this;
        foreach (var soLine in this)
        {
            var move = soLine.MoveIds.Where(m => m.RepairId != null && m.State == "done").ToList();
            if (move.Count != 1)
            {
                continue;
            }
            remainingSOLines = remainingSOLines.Where(s => s != soLine).ToList();
            soLine.QtyDelivered = move[0].Quantity;
        }
        Env.CallMethod("super", "_compute_qty_delivered", remainingSOLines);
    }

    public void Create(Dictionary<string, object> vals)
    {
        var res = Env.CallMethod("super", "create", vals);
        res.Where(line => line.State == "sale" || line.State == "done")._CreateRepairOrder();
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ProductUomQty"))
        {
            var oldProductUomQty = this.ToDictionary(line => line.Id, line => line.ProductUomQty);
            var res = Env.CallMethod("super", "write", vals);
            foreach (var line in this)
            {
                if (line.State == "sale" || line.State == "done" && line.ProductId != null)
                {
                    if (Env.FloatCompare(oldProductUomQty[line.Id], 0, line.ProductUom.Rounding) <= 0 && Env.FloatCompare(line.ProductUomQty, 0, line.ProductUom.Rounding) > 0)
                    {
                        this._CreateRepairOrder();
                    }
                    if (Env.FloatCompare(oldProductUomQty[line.Id], 0, line.ProductUom.Rounding) > 0 && Env.FloatCompare(line.ProductUomQty, 0, line.ProductUom.Rounding) <= 0)
                    {
                        this._CancelRepairOrder();
                    }
                }
            }
        }
        else
        {
            Env.CallMethod("super", "write", vals);
        }
    }

    public void ActionLaunchStockRule(double previousProductUomQty = 0)
    {
        var linesWithoutRepairMove = this.Where(line => line.MoveIds.All(m => m.RepairId == null)).ToList();
        Env.CallMethod("super", "_action_launch_stock_rule", linesWithoutRepairMove, previousProductUomQty);
    }

    public void _CreateRepairOrder()
    {
        var newRepairVals = new List<Dictionary<string, object>>();
        foreach (var line in this)
        {
            if (line.Order.RepairOrderIds.Any(ro => ro.SaleOrderLineId.Id == line.Id) && Env.FloatCompare(line.ProductUomQty, 0, line.ProductUom.Rounding) > 0)
            {
                var bindedRoIds = line.Order.RepairOrderIds.Where(ro => ro.SaleOrderLineId.Id == line.Id && ro.State == "cancel").ToList();
                bindedRoIds.ActionRepairCancelDraft();
                bindedRoIds.ActionRepairConfirm();
                continue;
            }
            if (!line.ProductTemplateId.CreateRepair || line.MoveIds.Any(m => m.RepairId != null) || Env.FloatCompare(line.ProductUomQty, 0, line.ProductUom.Rounding) <= 0)
            {
                continue;
            }

            var order = line.Order;
            var defaultRepairVals = new Dictionary<string, object>
            {
                { "State", "confirmed" },
                { "PartnerId", order.PartnerId.Id },
                { "SaleOrderId", order.Id },
                { "SaleOrderLineId", line.Id },
                { "PickingTypeId", order.WarehouseId.RepairTypeId.Id },
            };
            if (line.ProductId.Tracking == "serial")
            {
                var vals = new Dictionary<string, object>
                {
                    defaultRepairVals,
                    { "ProductId", line.ProductId.Id },
                    { "ProductQty", 1 },
                    { "ProductUom", line.ProductUom.Id },
                };
                newRepairVals.AddRange(Enumerable.Repeat(vals, (int)line.ProductUomQty));
            }
            else if (line.ProductId.Type == "consu")
            {
                newRepairVals.Add(new Dictionary<string, object>
                {
                    defaultRepairVals,
                    { "ProductId", line.ProductId.Id },
                    { "ProductQty", line.ProductUomQty },
                    { "ProductUom", line.ProductUom.Id },
                });
            }
            else
            {
                newRepairVals.Add(new Dictionary<string, object>(defaultRepairVals));
            }
        }

        if (newRepairVals.Any())
        {
            Env.Create("Repair.RepairOrder", newRepairVals);
        }
    }

    public void _CancelRepairOrder()
    {
        var bindedRoIds = new List<Repair.RepairOrder>();
        foreach (var line in this)
        {
            bindedRoIds.AddRange(line.Order.RepairOrderIds.Where(ro => ro.SaleOrderLineId.Id == line.Id && ro.State != "done").ToList());
        }
        bindedRoIds.ActionRepairCancel();
    }

    public bool HasValuedMoveIds()
    {
        var res = Env.CallMethod("super", "has_valued_move_ids");
        return res && this.MoveIds.All(m => m.RepairId == null);
    }
}
