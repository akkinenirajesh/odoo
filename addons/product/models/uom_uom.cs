csharp
public partial class ProductUoM 
{
    public void OnChangeRounding() 
    {
        decimal precision = Env.Services.Get("DecimalPrecision").GetPrecision("Product Unit of Measure");
        if (this.Rounding < 1.0 / Math.Pow(10, precision)) 
        {
            Env.UI.Warning(
                "Warning!",
                $"This rounding precision is higher than the Decimal Accuracy ({precision} digits).\nThis may cause inconsistencies in computations.\nPlease set a precision between {1.0 / Math.Pow(10, precision)} and 1."
            );
        }
    }
}
