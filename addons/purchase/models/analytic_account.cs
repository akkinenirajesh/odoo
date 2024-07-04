csharp
public partial class PurchaseAnalyticAccount 
{
    public int PurchaseOrderCount { get; set; }

    public void ComputePurchaseOrderCount()
    {
        this.PurchaseOrderCount = Env.Get<PurchaseOrder>().SearchCount(x => x.OrderLines.Any(y => y.InvoiceLines.Any(z => z.AnalyticLineIds.Any(a => a.AccountId == this.Id))));
    }

    public PurchaseOrder[] ActionViewPurchaseOrders()
    {
        var purchaseOrders = Env.Get<PurchaseOrder>().Search(x => x.OrderLines.Any(y => y.InvoiceLines.Any(z => z.AnalyticLineIds.Any(a => a.AccountId == this.Id))));
        return purchaseOrders;
    }
}
