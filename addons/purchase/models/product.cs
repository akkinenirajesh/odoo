csharp
public partial class PurchaseProductTemplate 
{
    public virtual void ComputePurchaseMethod()
    {
        var defaultPurchaseMethod = Env.GetModel("Purchase.ProductTemplate").DefaultGet(new string[] { "PurchaseMethod" })["PurchaseMethod"];
        if (this.Type == "service")
        {
            this.PurchaseMethod = "purchase";
        }
        else
        {
            this.PurchaseMethod = defaultPurchaseMethod;
        }
    }

    public virtual void ComputePurchasedProductQty()
    {
        this.PurchasedProductQty = Env.GetModel("Purchase.ProductProduct").Search(new string[] { "product_template_id", "=", this.Id }).Sum(p => p.PurchasedProductQty);
    }

    public virtual void ActionViewPo()
    {
        var action = Env.GetModel("Ir.Actions.Actions")._ForXmlId("purchase.action_purchase_history");
        action["domain"] = new string[] { "&", ("state", "in", new string[] { "purchase", "done" }), ("product_id", "in", this.ProductVariantIds.Select(p => p.Id).ToArray()) };
        action["display_name"] = string.Format("Purchase History for {0}", this.DisplayName);
    }
}

public partial class PurchaseProductProduct 
{
    public virtual void ComputePurchasedProductQty()
    {
        var dateFrom = Env.GetModel("Core.Datetime").ToString(Env.GetModel("Core.Date").ContextToday(this) - new RelativeDelta(years: 1));
        var domain = new string[] {
            ("order_id.state", "in", new string[] { "purchase", "done" }),
            ("product_id", "in", new string[] { this.Id }),
            ("order_id.date_approve", ">=", dateFrom)
        };
        var orderLines = Env.GetModel("Purchase.Order.Line")._ReadGroup(domain, new string[] { "product_id" }, new string[] { "product_uom_qty:sum" });
        var purchasedData = orderLines.ToDictionary(ol => ol["product_id"], ol => ol["product_uom_qty"]);
        this.PurchasedProductQty = purchasedData.ContainsKey(this.Id) ? (decimal)purchasedData[this.Id] : 0;
    }

    public virtual void ComputeIsInPurchaseOrder()
    {
        var orderId = Env.Context.Get("order_id");
        if (orderId == null)
        {
            this.IsInPurchaseOrder = false;
            return;
        }

        var readGroupData = Env.GetModel("Purchase.Order.Line")._ReadGroup(
            new string[] { ("order_id", "=", orderId) },
            new string[] { "product_id" },
            new string[] { "__count" });
        var data = readGroupData.ToDictionary(p => p["product_id"], p => p["__count"]);
        this.IsInPurchaseOrder = data.ContainsKey(this.Id) ? (int)data[this.Id] > 0 : false;
    }

    public virtual string[] SearchIsInPurchaseOrder(string operator, object value)
    {
        if (operator != "=" && operator != "!=")
        {
            throw new UserError("Operation not supported");
        }
        if (!(value is bool))
        {
            throw new UserError("Invalid value type");
        }
        var productIds = Env.GetModel("Purchase.Order.Line").Search(new string[] { ("order_id", "in", new string[] { Env.Context.Get("order_id") }) }).Select(p => p.ProductId).ToArray();
        return new string[] { ("id", "in", productIds) };
    }

    public virtual void ActionViewPo()
    {
        var action = Env.GetModel("Ir.Actions.Actions")._ForXmlId("purchase.action_purchase_history");
        action["domain"] = new string[] { "&", ("state", "in", new string[] { "purchase", "done" }), ("product_id", "in", this.Ids) };
        action["display_name"] = string.Format("Purchase History for {0}", this.DisplayName);
    }
}

public partial class PurchaseProductSupplierinfo
{
    public virtual void OnChangePartnerId()
    {
        this.CurrencyId = this.PartnerId.PropertyPurchaseCurrencyId.Id != null ? this.PartnerId.PropertyPurchaseCurrencyId.Id : Env.Company.CurrencyId.Id;
    }
}
