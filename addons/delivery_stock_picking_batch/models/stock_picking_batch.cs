csharp
public partial class StockPickingBatch
{
    public bool IsPickingAutoMergeable(Stock.Picking picking)
    {
        bool res = base.IsPickingAutoMergeable(picking);
        if (this.PickingTypeId.BatchMaxWeight > 0)
        {
            decimal batchWeight = this.PickingIds.Sum(p => p.Weight);
            res = res && (batchWeight + picking.Weight <= this.PickingTypeId.BatchMaxWeight);
        }
        return res;
    }
}
