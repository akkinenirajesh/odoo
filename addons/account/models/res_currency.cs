csharp
public partial class ResCurrency
{
    public string GetFiscalCountryCodes()
    {
        return string.Join(",", Env.Companies.Select(c => c.AccountFiscalCountry?.Code).Where(c => c != null));
    }

    public void ComputeDisplayRoundingWarning()
    {
        DisplayRoundingWarning = Id != 0 && Origin.Rounding != Rounding && HasAccountingEntries();
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Rounding"))
        {
            decimal roundingVal = (decimal)vals["Rounding"];
            if ((roundingVal > Rounding || roundingVal == 0) && HasAccountingEntries())
            {
                throw new UserError("You cannot reduce the number of decimal places of a currency which has already been used to make accounting entries.");
            }
        }

        return base.Write(vals);
    }

    public bool HasAccountingEntries()
    {
        return Env.AccountMoveLines.Sudo().Any(aml => aml.Currency.Id == Id || aml.CompanyCurrency.Id == Id);
    }

    public SQL GetQueryCurrencyTable(List<int> companyIds, DateTime conversionDate)
    {
        var companies = Env.Companies.Browse(companyIds);
        var userCompany = Env.Company;

        Dictionary<int, decimal> currencyRates;
        if (companies.SequenceEqual(new[] { userCompany }))
        {
            currencyRates = new Dictionary<int, decimal> { { userCompany.Currency.Id, 1.0m } };
        }
        else
        {
            currencyRates = companies.Select(c => c.Currency).GetRates(userCompany, conversionDate);
        }

        var conversionRates = new List<object>();
        foreach (var company in companies)
        {
            conversionRates.Add(company.Id);
            conversionRates.Add(currencyRates[userCompany.Currency.Id] / currencyRates[company.Currency.Id]);
            conversionRates.Add(userCompany.Currency.DecimalPlaces);
        }

        string query = $"(VALUES {string.Join(",", Enumerable.Repeat("(%s, %s, %s)", companies.Count()))}) AS currency_table(company_id, rate, precision)";
        return new SQL(query, conversionRates.ToArray());
    }
}
