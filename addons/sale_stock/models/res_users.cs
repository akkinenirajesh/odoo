csharp
public partial class ResUsers {
    public virtual Stock.Warehouse PropertyWarehouseId { get; set; }

    public ResUsers GetDefaultWarehouseId() {
        if (this.PropertyWarehouseId != null) {
            return this.PropertyWarehouseId;
        }
        return Env.GetDefaultWarehouseId();
    }
}
