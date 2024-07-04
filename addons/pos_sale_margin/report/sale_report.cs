csharp
public partial class SaleReport
{
    public void ComputeMargin()
    {
        this.Margin = (decimal)Env.Compute("SUM(l.PriceSubtotal - COALESCE(l.TotalCost, 0) / CASE COALESCE(pos.CurrencyRate, 0) WHEN 0 THEN 1.0 ELSE pos.CurrencyRate END)");
    }
}
