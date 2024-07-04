csharp
public partial class PurchaseStock.StockValuationLayer
{
    public decimal GetLayerPriceUnit()
    {
        return this.Value / this.Quantity;
    }
}
