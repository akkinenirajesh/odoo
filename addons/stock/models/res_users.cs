csharp
public partial class Users {
    public Stock.Warehouse GetDefaultWarehouseId() {
        return Env.GetModel<Stock.Warehouse>().Search(x => x.CompanyId == Env.Company.Id).FirstOrDefault();
    }
}
