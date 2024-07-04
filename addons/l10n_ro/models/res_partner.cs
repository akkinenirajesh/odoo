csharp
public partial class ResPartner {
    public string NRC { get; set; }

    public void ComputeCompanyRegistry() {
        if (Env.Context.Contains("company_id") && this.Country.Code == "RO" && !string.IsNullOrEmpty(this.VAT)) {
            var vatCountry = string.Empty;
            var vatNumber = string.Empty;
            if (SplitVAT(this.VAT, out vatCountry, out vatNumber)) {
                if (vatCountry.IsNumeric() && this.SimpleVATCheck("ro", vatNumber)) {
                    this.CompanyRegistry = vatNumber;
                }
            }
        }
    }

    private bool SplitVAT(string vat, out string vatCountry, out string vatNumber) {
        // implementation of split vat
        vatCountry = string.Empty;
        vatNumber = string.Empty;
        return false;
    }

    private bool SimpleVATCheck(string countryCode, string vatNumber) {
        // implementation of simple vat check
        return false;
    }
}
