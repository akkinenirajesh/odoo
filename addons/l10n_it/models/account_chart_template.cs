csharp
public partial class AccountChartTemplate
{
    public object Load(Company company)
    {
        var res = base.Load(company);
        if (company.AccountFiscalCountryId.Code == "IT")
        {
            company.Write(new Dictionary<string, object>
            {
                { "TaxCalculationRoundingMethod", "round_globally" }
            });
        }
        return res;
    }
}
