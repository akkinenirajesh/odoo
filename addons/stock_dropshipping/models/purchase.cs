csharp
public partial class Stock.PurchaseOrder
{
    public int DropshipPickingCount { get; set; }

    public void ComputeDropshipPickingCount()
    {
        this.DropshipPickingCount = Env.Model("Stock.Picking").Search(x => x.IsDropship && x.PurchaseOrderId == this.Id).Count;
    }

    public void ActionViewPicking()
    {
        var pickings = Env.Model("Stock.Picking").Search(x => x.PurchaseOrderId == this.Id && !x.IsDropship);
        // ...
    }

    public void ActionViewDropship()
    {
        var pickings = Env.Model("Stock.Picking").Search(x => x.PurchaseOrderId == this.Id && x.IsDropship);
        // ...
    }
}

public partial class Stock.PurchaseOrderLine
{
    public void PrepareStockMoves(Stock.Picking picking)
    {
        // ...
    }

    public Stock.PurchaseOrderLine FindCandidate(int productId, decimal productQty, int productUomId, int locationId, string name, string origin, int companyId, dynamic values)
    {
        // ...
    }

    public void PreparePurchaseOrderLineFromProcurement(int productId, decimal productQty, int productUomId, int locationDestId, string name, string origin, int companyId, dynamic values, Stock.PurchaseOrder purchaseOrder)
    {
        // ...
    }
}
