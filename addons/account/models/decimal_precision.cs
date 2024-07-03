csharp
public partial class DecimalPrecision
{
    public decimal PrecisionGet(string application)
    {
        if (application == "Discount" && Env.Context.GetValueOrDefault("ignore_discount_precision", false))
        {
            return 100;
        }
        return base.PrecisionGet(application);
    }
}
