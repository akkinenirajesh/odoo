csharp
public partial class Bank 
{
    public string L10nMxEdiCode { get; set; }
    public string FiscalCountryCodes { get; set; }

    private string _get_fiscal_country_codes() {
        // Access the environment to get companies
        var companies = Env.Get("Core.Company").GetAll();
        
        // Map the companies to their fiscal country code
        var fiscalCountryCodes = companies.Select(c => c.Get("AccountFiscalCountryId").Get("Code").ToString());

        // Join the codes with a comma
        return string.Join(",", fiscalCountryCodes);
    }
}

public partial class ResPartnerBank
{
    public string L10nMxEdiClabe { get; set; }
    public string FiscalCountryCodes { get; set; }

    private string _get_fiscal_country_codes()
    {
        // Access the environment to get companies
        var companies = Env.Get("Core.Company").GetAll();
        
        // Map the companies to their fiscal country code
        var fiscalCountryCodes = companies.Select(c => c.Get("AccountFiscalCountryId").Get("Code").ToString());

        // Join the codes with a comma
        return string.Join(",", fiscalCountryCodes);
    }
}
