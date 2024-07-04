csharp
public partial class CurrencyRate
{
    public void OnChangeCompanyRate()
    {
        if (this.Company.AccountFiscalCountry?.Code == "EG" &&
            FloatCompare(this.InverseCompanyRate, Math.Round(this.InverseCompanyRate, 5), 10) != 0)
        {
            Env.ShowWarning(
                $"Warning for {this.Currency.Name}",
                "Please make sure that the EGP per unit is within 5 decimal accuracy.\n" +
                "Higher decimal accuracy might lead to inconsistency with the ETA invoicing portal!"
            );
        }
    }

    private int FloatCompare(decimal value1, decimal value2, int precisionDigits)
    {
        // Implement float comparison logic similar to Odoo's float_compare
        decimal epsilon = (decimal)Math.Pow(10, -precisionDigits);
        if (Math.Abs(value1 - value2) < epsilon)
            return 0;
        return value1 > value2 ? 1 : -1;
    }
}
