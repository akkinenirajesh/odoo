csharp
public partial class StockSaleOrder 
{
    public int DropshipPickingCount { get; set; }
    public int DeliveryCount { get; set; }

    public void ComputePickingIds()
    {
        this.DeliveryCount = this.PickingIds.Count(p => !p.IsDropship);
        this.DropshipPickingCount = this.PickingIds.Count(p => p.IsDropship);
    }

    public object ActionViewDelivery()
    {
        return _GetActionViewPicking(this.PickingIds.Where(p => !p.IsDropship));
    }

    public object ActionViewDropship()
    {
        return _GetActionViewPicking(this.PickingIds.Where(p => p.IsDropship));
    }

    private object _GetActionViewPicking(IEnumerable<StockPicking> pickings)
    {
        // Implement logic based on pickings
        return null; 
    }
}

public partial class StockSaleOrderLine
{
    public bool IsMTO { get; set; }

    public void ComputeIsMTO()
    {
        if (this.DisplayQtyWidget || this.IsMTO)
        {
            return;
        }

        foreach (var pullRule in this.RouteId?.RuleIds ?? this.ProductId.RouteIds.Union(this.CategId.TotalRouteIds))
        {
            if (pullRule.PickingTypeId.DefaultLocationSrcId.Usage == "supplier" &&
                pullRule.PickingTypeId.DefaultLocationDestId.Usage == "customer")
            {
                this.IsMTO = true;
                break;
            }
        }
    }

    public decimal GetQtyProcurement(decimal previousProductUomQty)
    {
        if (this.PurchaseLineIds.Any(r => r.State != "cancel"))
        {
            decimal qty = 0;
            foreach (var poLine in this.PurchaseLineIds.Where(r => r.State != "cancel"))
            {
                qty += poLine.ProductUom.ComputeQuantity(poLine.ProductQty, this.ProductUom, "HALF-UP");
            }
            return qty;
        }
        else
        {
            return base.GetQtyProcurement(previousProductUomQty); 
        }
    }

    public void ComputeProductUpdatable()
    {
        if (Env.User.HasGroup("purchase.group_purchase_user"))
        {
            if (this.PurchaseLineCount > 0)
            {
                this.ProductUpdatable = false;
            }
        }
    }
}
