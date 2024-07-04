csharp
public partial class StockMove
{
    public decimal L10nInGetProductPriceUnit()
    {
        if (SaleLine != null)
        {
            if (SaleLine.ProductUomQty != 0)
            {
                return SaleLine.PriceSubtotal / SaleLine.ProductUomQty;
            }
            return 0.00m;
        }
        return base.L10nInGetProductPriceUnit();
    }

    public Dictionary<string, object> L10nInGetProductTax()
    {
        if (SaleLine != null)
        {
            return new Dictionary<string, object>
            {
                { "IsFromOrder", true },
                { "Taxes", SaleLine.TaxId }
            };
        }
        return base.L10nInGetProductTax();
    }
}
