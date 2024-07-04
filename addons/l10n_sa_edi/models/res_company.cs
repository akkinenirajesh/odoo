C#
public partial class ResCompany
{
    public byte[] L10nSaPrivateKey { get; set; }

    public string L10nSaApiMode { get; set; }

    public string L10nSaEdiBuildingNumber { get; set; }

    public string L10nSaEdiPlotIdentification { get; set; }

    public Core.Country L10nSaAdditionalIdentificationScheme { get; set; }

    public string L10nSaAdditionalIdentificationNumber { get; set; }

    public ResPartner PartnerId { get; set; }

    public byte[] _L10nSaGeneratePrivateKey()
    {
        // Implement logic to generate private key using C# cryptography libraries
        // ...
    }

    public void ComputeAddress()
    {
        // Implement logic to compute L10nSaEdiBuildingNumber and L10nSaEdiPlotIdentification based on partner_id address
        // ...
    }

    public void L10nSaEdiInverseBuildingNumber()
    {
        this.PartnerId.L10nSaEdiBuildingNumber = this.L10nSaEdiBuildingNumber;
    }

    public void L10nSaEdiInversePlotIdentification()
    {
        this.PartnerId.L10nSaEdiPlotIdentification = this.L10nSaEdiPlotIdentification;
    }

    public string _L10nSaGetCsrInvoiceType()
    {
        return "1100";
    }

    public bool _L10nSaCheckOrganizationUnit()
    {
        if (string.IsNullOrEmpty(this.Vat))
        {
            return false;
        }

        return this.Vat.Length == 15 && System.Text.RegularExpressions.Regex.IsMatch(this.Vat, "^3\\d{13}3$");
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("L10nSaApiMode"))
        {
            if (this.L10nSaApiMode == "prod" && vals["L10nSaApiMode"] != "prod")
            {
                throw new Exception("You cannot change the ZATCA Submission Mode once it has been set to Production");
            }

            List<AccountJournal> journals = Env.Search<AccountJournal>().Where(j => j.Company.Id == this.Id).ToList();
            journals.ForEach(j => j._L10nSaResetCertificates());
            journals.ForEach(j => j.L10nSaLatestSubmissionHash = null);
        }

        // Implement remaining write logic
    }

    public List<string> _GetCompanyRootDelegatedFieldNames()
    {
        return new List<string>()
        {
            "L10nSaApiMode",
            "L10nSaPrivateKey"
        };
    }

    public List<string> _GetCompanyAddressFieldNames()
    {
        return new List<string>()
        {
            "L10nSaEdiBuildingNumber",
            "L10nSaEdiPlotIdentification"
        };
    }
}
