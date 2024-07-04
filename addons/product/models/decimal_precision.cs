C#
public partial class ProductDecimalPrecision
{
    public void CheckMainCurrencyRounding()
    {
        if (this.Name == "Account" && Env.Company.CurrencyId.Rounding < Math.Pow(10, -this.Digits))
        {
            throw new Exception("You cannot define the decimal precision of 'Account' as greater than the rounding factor of the company's main currency");
        }
    }

    public void OnChangeDigits()
    {
        if (this.Name != "Product Unit of Measure")
        {
            return;
        }

        decimal rounding = 1.0m / (decimal)Math.Pow(10, this.Digits);
        var dangerousUom = Env.Search<UomUom>().Where(uom => uom.Rounding < rounding).ToList();

        if (dangerousUom.Any())
        {
            List<string> uomDescriptions = dangerousUom.Select(uom => $" - {uom.Name} (id={uom.Id}, precision={uom.Rounding})").ToList();
            throw new Exception($"You are setting a Decimal Accuracy less precise than the UOMs:\n{string.Join("\n", uomDescriptions)}\nThis may cause inconsistencies in computations.\nPlease increase the rounding of those units of measure, or the digits of this Decimal Accuracy.");
        }
    }
}
