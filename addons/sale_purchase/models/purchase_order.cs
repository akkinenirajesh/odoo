csharp
public partial class SalePurchase.PurchaseOrder
{
    public int SaleOrderCount { get; set; }

    public IEnumerable<SalePurchase.PurchaseOrderLine> OrderLines { get; set; }

    public void ComputeSaleOrderCount()
    {
        this.SaleOrderCount = GetSaleOrders().Count();
    }

    public void ActionViewSaleOrders()
    {
        var saleOrderIds = GetSaleOrders().Select(x => x.Id).ToList();
        var action = new {
            resModel = "Sale.SaleOrder",
            type = "ir.actions.act_window"
        };

        if (saleOrderIds.Count == 1)
        {
            action = new {
                resModel = "Sale.SaleOrder",
                type = "ir.actions.act_window",
                viewMode = "form",
                resId = saleOrderIds[0]
            };
        }
        else
        {
            action = new {
                resModel = "Sale.SaleOrder",
                type = "ir.actions.act_window",
                name = $"Sources Sale Orders {this.Name}",
                domain = new[] { ("Id", "in", saleOrderIds) },
                viewMode = "tree,form"
            };
        }
    }

    public void ButtonCancel()
    {
        var result = Env.Call("super", "ButtonCancel");
        ActivityCancelOnSale();
    }

    private IEnumerable<Sale.SaleOrder> GetSaleOrders()
    {
        return this.OrderLines.Select(x => x.SaleOrderId);
    }

    private void ActivityCancelOnSale()
    {
        var saleToNotifyMap = new Dictionary<Sale.SaleOrder, List<SalePurchase.PurchaseOrderLine>>();

        foreach (var purchaseLine in this.OrderLines)
        {
            if (purchaseLine.SaleLineId != null)
            {
                var saleOrder = purchaseLine.SaleLineId.OrderId;
                if (!saleToNotifyMap.ContainsKey(saleOrder))
                {
                    saleToNotifyMap.Add(saleOrder, new List<SalePurchase.PurchaseOrderLine>());
                }
                saleToNotifyMap[saleOrder].Add(purchaseLine);
            }
        }

        foreach (var saleOrder in saleToNotifyMap.Keys)
        {
            saleOrder.ActivityScheduleWithView("mail.mail_activity_data_warning",
                userId: saleOrder.UserId ?? Env.Uid,
                viewsOrXmlid: "sale_purchase.exception_sale_on_purchase_cancellation",
                renderContext: new {
                    purchaseOrders = saleToNotifyMap[saleOrder].Select(x => x.OrderId).ToList(),
                    purchaseOrderLines = saleToNotifyMap[saleOrder]
                }
            );
        }
    }
}
