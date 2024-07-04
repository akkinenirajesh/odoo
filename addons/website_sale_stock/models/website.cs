csharp
public partial class Website 
{
    public Website(Buvi.Env env) : base(env) { }

    public int WarehouseId { get; set; }

    public Dictionary<string, object> PrepareSaleOrderValues(Partner partnerSudo)
    {
        var values = base.PrepareSaleOrderValues(partnerSudo);

        var warehouseId = GetWarehouseAvailable();
        if (warehouseId > 0)
        {
            values["WarehouseId"] = warehouseId;
        }
        return values;
    }

    public int GetWarehouseAvailable()
    {
        return (
            WarehouseId > 0 ? WarehouseId :
            Env.IrDefault.Get("sale.order", "warehouse_id", company_id: this.CompanyId) ??
            Env.IrDefault.Get("sale.order", "warehouse_id") ??
            Env.StockWarehouse.Search(new List<object>() { ("company_id", "=", this.CompanyId) }, limit: 1).First().Id
        );
    }

    public SaleOrder SaleGetOrder(params object[] args)
    {
        var so = base.SaleGetOrder(args);
        return so != null ? so.WithContext(new Dictionary<string, object>() { ("warehouse_id", so.WarehouseId) }) : so;
    }

    public decimal GetProductAvailableQty(Product product, Dictionary<string, object> kwargs)
    {
        return product.WithContext(new Dictionary<string, object>() { ("warehouse_id", GetWarehouseAvailable()) }).FreeQty;
    }
}
