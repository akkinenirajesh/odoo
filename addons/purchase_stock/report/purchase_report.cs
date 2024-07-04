csharp
public partial class PurchaseReport 
{
    public virtual DateTime EffectiveDate { get; set; }

    public virtual Stock.Warehouse PickingTypeId { get; set; }
}
