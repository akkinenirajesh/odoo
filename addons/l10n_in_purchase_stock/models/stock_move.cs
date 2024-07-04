csharp
public partial class StockMove
{
    public decimal L10nInGetProductPriceUnit()
    {
        if (this.PurchaseLineId != null)
        {
            if (this.PurchaseLineId.ProductQty != 0)
            {
                return this.PurchaseLineId.PriceSubtotal / this.PurchaseLineId.ProductQty;
            }
            return 0.00m;
        }
        return base.L10nInGetProductPriceUnit();
    }

    public object L10nInGetProductTax()
    {
        if (this.PurchaseLineId != null)
        {
            return new
            {
                IsFromOrder = true,
                Taxes = this.PurchaseLineId.TaxesId
            };
        }
        return base.L10nInGetProductTax();
    }
}
