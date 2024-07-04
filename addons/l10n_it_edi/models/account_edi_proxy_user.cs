csharp
public partial class AccountEdiProxyClientUser
{
    public Dictionary<string, Dictionary<string, string>> GetProxyUrls()
    {
        var urls = base.GetProxyUrls(); // Assuming base class method exists
        urls["L10nItEdi"] = new Dictionary<string, string>
        {
            { "demo", "" },
            { "prod", "https://l10n-it-edi.api.odoo.com" },
            { "test", "https://iap-services-test.odoo.com" }
        };
        return urls;
    }

    public string GetProxyIdentification(Core.Company company, string proxyType)
    {
        if (proxyType == "L10nItEdi")
        {
            if (string.IsNullOrEmpty(company.L10nItCodiceFiscale))
            {
                throw new UserException("Please fill your codice fiscale to be able to receive invoices from FatturaPA");
            }
            return company.Partner.L10nItEdiNormalizedCodiceFiscale();
        }
        return base.GetProxyIdentification(company, proxyType);
    }
}
