csharp
public partial class AccountFiscalPosition
{
    public void ValidateForeignVat()
    {
        if (string.IsNullOrEmpty(ForeignVat))
        {
            return;
        }

        var partner = Env.Get<ResPartner>();
        var checkedCountryCode = partner.RunVatTest(ForeignVat, CountryId);

        if (!string.IsNullOrEmpty(checkedCountryCode) && checkedCountryCode != CountryId.Code.ToLower())
        {
            throw new ValidationException("The country detected for this foreign VAT number does not match the one set on this fiscal position.");
        }

        if (string.IsNullOrEmpty(checkedCountryCode))
        {
            var fpLabel = $"fiscal position [{Name}]";
            var errorMessage = partner.BuildVatErrorMessage(CountryId.Code.ToLower(), ForeignVat, fpLabel);
            throw new ValidationException(errorMessage);
        }
    }

    public bool GetVatValid(ResPartner delivery, Company company = null)
    {
        var euCountries = Env.Ref<CountryGroup>("base.europe").Countries;

        if (!(company != null && delivery.WithCompany(company).PerformViesValidation))
        {
            return base.GetVatValid(delivery, company);
        }

        var fiscalPositionSearch = Env.Search<AccountFiscalPosition>(new[]
        {
            CheckCompanyDomain(company),
            ("ForeignVat", "!=", null),
            ("CountryId", "=", delivery.CountryId.Id)
        });

        if (fiscalPositionSearch.Any() || euCountries.Contains(company.CountryId))
        {
            return base.GetVatValid(delivery, company) && delivery.ViesValid;
        }

        return base.GetVatValid(delivery, company);
    }

    private Expression<Func<AccountFiscalPosition, bool>> CheckCompanyDomain(Company company)
    {
        // Implement the logic for _check_company_domain here
        // This is a placeholder and should be replaced with actual implementation
        return fp => true;
    }
}
