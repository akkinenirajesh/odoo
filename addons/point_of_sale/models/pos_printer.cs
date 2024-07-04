csharp
public partial class PointOfSalePosPrinter {
    public virtual void LoadPosDataDomain(dynamic data) {
        var ids = data["pos.config"]["data"][0]["printer_ids"];
        return new object[] { "Id", "in", ids };
    }

    public virtual object[] LoadPosDataFields(int configId) {
        return new object[] { "Id", "Name", "ProxyIP", "ProductCategories", "PrinterType" };
    }
}
