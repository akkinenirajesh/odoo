csharp
public partial class ResCompany
{
    public override async Task<IEnumerable<ResCompany>> Create(IEnumerable<ResCompany> companies)
    {
        var createdCompanies = await base.Create(companies);

        foreach (var company in createdCompanies)
        {
            if (company.CountryId == null)
            {
                continue;
            }

            var countryVatType = await Env.Set<L10nLatam.IdentificationType>()
                .Where(t => t.IsVat && t.CountryId == company.CountryId)
                .FirstOrDefaultAsync();

            if (countryVatType != null)
            {
                company.PartnerId.L10nLatamIdentificationTypeId = countryVatType;
            }
        }

        return createdCompanies;
    }
}
