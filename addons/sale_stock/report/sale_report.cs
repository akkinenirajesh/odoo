C#
public partial class SaleReport {
    public SaleReport() { }

    public virtual Stock.Warehouse WarehouseId { get; set; }

    public virtual void _SelectAdditionalFields() {
        var res = Env.Call<SaleReport>("_SelectAdditionalFields");
        res["WarehouseId"] = "s.warehouse_id";
        // ...rest of the implementation
    }

    public virtual void _GroupBySale() {
        var res = Env.Call<SaleReport>("_GroupBySale");
        res += ",\n            s.warehouse_id";
        // ...rest of the implementation
    }
}
