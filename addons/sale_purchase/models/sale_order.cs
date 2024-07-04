csharp
public partial class SaleOrder 
{
    public int PurchaseOrderCount { get; set; }

    public void ComputePurchaseOrderCount()
    {
        this.PurchaseOrderCount = this.GetPurchaseOrders().Count;
    }

    public void ActionConfirm()
    {
        base.ActionConfirm();
        foreach (var orderLine in this.OrderLines)
        {
            orderLine.PurchaseServiceGeneration();
        }
    }

    public void ActionCancel()
    {
        base.ActionCancel();
        this.ActivityCancelOnPurchase();
    }

    public dynamic ActionViewPurchaseOrders()
    {
        var purchaseOrderIds = this.GetPurchaseOrders().Select(po => po.Id).ToList();
        var action = new { resModel = "Purchase.PurchaseOrder", type = "ir.actions.act_window" };
        if (purchaseOrderIds.Count == 1)
        {
            action = new { resModel = "Purchase.PurchaseOrder", type = "ir.actions.act_window", viewMode = "form", resId = purchaseOrderIds[0] };
        }
        else
        {
            action = new { resModel = "Purchase.PurchaseOrder", type = "ir.actions.act_window", name = $"Purchase Order generated from {this.Name}", domain = new[] { new[] { "Id", "in", purchaseOrderIds } }, viewMode = "tree,form" };
        }
        return action;
    }

    private List<PurchaseOrder> GetPurchaseOrders()
    {
        return this.OrderLines.SelectMany(ol => ol.PurchaseLines).Select(pl => pl.Order).ToList();
    }

    private void ActivityCancelOnPurchase()
    {
        var purchaseToNotifyMap = new Dictionary<PurchaseOrder, List<SaleOrderLine>>();
        var purchaseOrderLines = Env.Search<PurchaseOrderLine>(new[] { new[] { "SaleLineId", "in", this.OrderLines.Select(ol => ol.Id) }, new[] { "State", "!=", "cancel" } });
        foreach (var purchaseLine in purchaseOrderLines)
        {
            if (!purchaseToNotifyMap.ContainsKey(purchaseLine.Order))
            {
                purchaseToNotifyMap.Add(purchaseLine.Order, new List<SaleOrderLine>());
            }
            purchaseToNotifyMap[purchaseLine.Order].Add(purchaseLine.SaleLine);
        }

        foreach (var purchaseOrder in purchaseToNotifyMap.Keys)
        {
            purchaseOrder.ActivityScheduleWithView("mail.mail_activity_data_warning",
                userId: purchaseOrder.UserId ?? Env.UserId,
                viewsOrXmlid: "sale_purchase.exception_purchase_on_sale_cancellation",
                renderContext: new { saleOrders = purchaseToNotifyMap[purchaseOrder].Select(sol => sol.Order).ToList(), saleOrderLines = purchaseToNotifyMap[purchaseOrder] });
        }
    }
}
