C#
public partial class StockValuationLayer
{
    public virtual decimal GetLayerPriceUnit()
    {
        decimal componentsPrice = 0;
        var production = this.StockMoveId.FirstOrDefault()?.ProductionId;
        if (production != null && production.SubcontractorId != null && production.State == "done")
        {
            foreach (var move in production.MoveRawIds)
            {
                componentsPrice += move.StockValuationLayerIds.Sum(layer => Math.Abs(layer.Value)) / production.ProductUomQty;
            }
        }

        return Env.Call<decimal>("stock.valuation.layer", "_get_layer_price_unit") - componentsPrice;
    }
}
