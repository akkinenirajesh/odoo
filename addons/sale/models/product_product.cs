csharp
public partial class SaleProductProduct
{
    public void ComputeSalesCount()
    {
        if (!Env.User.IsInGroup("sales_team.group_sale_salesman"))
        {
            return;
        }
        var dateFrom = DateTime.Now.AddDays(-365);
        var doneStates = Env.GetModel("sale.report").GetDoneStates();
        var domain = new List<object>()
        {
            new List<object>() { "state", "in", doneStates },
            new List<object>() { "product_id", "in", this.Ids },
            new List<object>() { "date", ">=", dateFrom },
        };
        var readGroupData = Env.GetModel("sale.report").ReadGroup(domain, new List<string>() { "product_id" },
            new List<string>() { "product_uom_qty:sum" });
        var r = new Dictionary<long, double>();
        foreach (var item in readGroupData)
        {
            var product = item["product_id"];
            var productUomQty = item["product_uom_qty:sum"];
            r.Add((long)product, (double)productUomQty);
        }
        this.SalesCount = r.TryGetValue(this.Id, out var result) ? result : 0;
    }

    public void OnChangeType()
    {
        if (this.SalesCount > 0)
        {
            Env.NotifyWarning("Warning", "You cannot change the product's type because it is already used in sales orders.");
        }
    }

    public void ComputeProductIsInSaleOrder()
    {
        var orderID = Env.Context.Get<long>("order_id");
        if (orderID == 0)
        {
            this.ProductCatalogProductIsInSaleOrder = false;
            return;
        }
        var readGroupData = Env.GetModel("sale.order.line").ReadGroup(
            new List<object>() { new List<object>() { "order_id", "=", orderID } },
            new List<string>() { "product_id" },
            new List<string>() { "__count" });
        var data = new Dictionary<long, int>();
        foreach (var item in readGroupData)
        {
            var product = item["product_id"];
            var count = item["__count"];
            data.Add((long)product, (int)count);
        }
        this.ProductCatalogProductIsInSaleOrder = data.TryGetValue(this.Id, out var result) ? result > 0 : false;
    }

    public List<object> SearchProductIsInSaleOrder(string operator, bool value)
    {
        if (operator != "=" && operator != "!=")
        {
            throw new Exception("Operation not supported");
        }
        var orderID = Env.Context.Get<long>("order_id");
        var productIds = Env.GetModel("sale.order.line").Search(new List<object>()
        {
            new List<object>() { "order_id", "in", new List<long>() { orderID } }
        }).Select(x => x.Id).ToList();
        return new List<object>() { new List<object>() { "id", "in", productIds } };
    }

    public object ActionViewSales()
    {
        var action = Env.GetModel("ir.actions.actions").GetActionForXmlId("sale.report_all_channels_sales_action");
        action["domain"] = new List<object>() { new List<object>() { "product_id", "in", this.Ids } };
        action["context"] = new Dictionary<string, object>()
        {
            { "pivot_measures", new List<string>() { "product_uom_qty" } },
            { "active_id", Env.Context.Get<long>("active_id") },
            { "search_default_Sales", 1 },
            { "active_model", "sale.report" },
            { "search_default_filter_order_date", 1 },
        };
        return action;
    }

    public List<long> GetBackendRootMenuIds()
    {
        var baseMenuIds = base.GetBackendRootMenuIds();
        baseMenuIds.Add(Env.GetReference("sale.sale_menu_root"));
        return baseMenuIds;
    }

    public object GetInvoicePolicy()
    {
        return this.InvoicePolicy;
    }

    public List<SaleProductProduct> FilterToUnlink()
    {
        var domain = new List<object>() { new List<object>() { "product_id", "in", this.Ids } };
        var lines = Env.GetModel("sale.order.line").ReadGroup(domain, new List<string>() { "product_id" });
        var linkedProductIds = lines.Select(x => (long)x["product_id"]).ToList();
        return (SaleProductProduct)this - (SaleProductProduct)this.Env.GetRecords<SaleProductProduct>(linkedProductIds);
    }
}
