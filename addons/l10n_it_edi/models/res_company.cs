csharp
public partial class ResCompany
{
    public override string ToString()
    {
        return Name;
    }

    public void OnChangeL10nItHasTaxRepresentative()
    {
        if (!L10nItHasTaxRepresentative)
        {
            L10nItTaxRepresentativePartnerId = null;
        }
    }

    public Dictionary<string, object> L10nItEdiExportCheck()
    {
        var errors = new Dictionary<string, object>();

        // Company VAT or Codice Fiscale check
        if (string.IsNullOrEmpty(Vat) && string.IsNullOrEmpty(L10nItCodiceFiscale))
        {
            errors["l10n_it_edi_company_vat_codice_fiscale_missing"] = new
            {
                message = "Company should have a VAT number or Codice Fiscale.",
                action_text = "View Company",
                action = GetRecordsAction("Check Company Data")
            };
        }

        // Company address check
        if (string.IsNullOrEmpty(Street) && string.IsNullOrEmpty(Street2) ||
            string.IsNullOrEmpty(Zip) || string.IsNullOrEmpty(City) || CountryId == null)
        {
            errors["l10n_it_edi_company_address_missing"] = new
            {
                message = "Company should have a complete address, verify their Street, City, Zipcode and Country.",
                action_text = "View Company",
                action = GetRecordsAction("Check Company Data")
            };
        }

        // Tax System check
        if (L10nItTaxSystem == null)
        {
            errors["l10n_it_edi_company_l10n_it_tax_system_missing"] = new
            {
                message = "Company should have a Tax System",
                action_text = "View Company",
                action = GetRecordsAction("Check Company Data")
            };
        }

        // EDI Proxy User check
        if (L10nItEdiProxyUserId == null)
        {
            errors["l10n_it_edi_settings_l10n_it_edi_proxy_user_id"] = new
            {
                message = "You must accept the terms and conditions in the Settings to use the IT EDI.",
                action_text = "View Settings",
                action = Env.Get<ResConfigSettings>().GetRecordsAction("Settings", new { module = "account", default_search_setting = "Italian Electronic Invoicing", bin_size = false })
            };
        }

        return errors;
    }
}
