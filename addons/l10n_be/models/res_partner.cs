csharp
public partial class ResPartner
{
    public string _ComputeCompanyRegistry()
    {
        // OVERRIDE
        // If a belgian company has a VAT number then its company registry is its VAT Number (without country code).
        base._ComputeCompanyRegistry();
        
        if (this._DeduceCountryCode() == "BE" && !string.IsNullOrEmpty(this.Vat))
        {
            var (vatCountry, vatNumber) = this._SplitVat(this.Vat);
            if (int.TryParse(vatCountry, out _))
            {
                vatCountry = "be";
                vatNumber = this.Vat;
            }
            if (vatCountry == "be" && this.SimpleVatCheck(vatCountry, vatNumber))
            {
                return vatNumber;
            }
        }
        
        return this.CompanyRegistry;
    }

    private string _DeduceCountryCode()
    {
        // Implement the logic to deduce country code
        // This is a placeholder and should be replaced with actual implementation
        return this.CountryId?.Code ?? "";
    }

    private (string, string) _SplitVat(string vat)
    {
        // Implement the logic to split VAT into country code and number
        // This is a placeholder and should be replaced with actual implementation
        if (string.IsNullOrEmpty(vat) || vat.Length < 2)
            return ("", "");
        return (vat.Substring(0, 2), vat.Substring(2));
    }

    private bool SimpleVatCheck(string countryCode, string vatNumber)
    {
        // Implement the simple VAT check logic
        // This is a placeholder and should be replaced with actual implementation
        return true;
    }
}
