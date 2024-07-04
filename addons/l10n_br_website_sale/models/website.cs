csharp
public partial class Website
{
    public override IEnumerable<Website> Create(IEnumerable<Dictionary<string, object>> valsList)
    {
        foreach (var website in valsList)
        {
            if (website.TryGetValue("CompanyId", out var companyId))
            {
                var company = Env.Get<Core.Company>().Browse((int)companyId);
                if (company.CountryCode == "BR")
                {
                    if (!website.ContainsKey("ShowLineSubtotalsTaxSelection"))
                    {
                        website["ShowLineSubtotalsTaxSelection"] = "tax_included";
                    }
                }
            }
        }
        return base.Create(valsList);
    }
}
