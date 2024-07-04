C#
public partial class ResPartner
{
    public string L10nSaEdiBuildingNumber { get; set; }
    public string L10nSaEdiPlotIdentification { get; set; }
    public string L10nSaAdditionalIdentificationScheme { get; set; }
    public string L10nSaAdditionalIdentificationNumber { get; set; }

    public List<string> _CommercialFields()
    {
        return Env.GetFields("Res.Partner").Concat(new List<string>()
        {
            "L10nSaEdiBuildingNumber",
            "L10nSaEdiPlotIdentification",
            "L10nSaAdditionalIdentificationScheme",
            "L10nSaAdditionalIdentificationNumber"
        }).ToList();
    }

    public List<string> _AddressFields()
    {
        return Env.GetFields("Res.Partner").Concat(new List<string>()
        {
            "L10nSaEdiBuildingNumber",
            "L10nSaEdiPlotIdentification"
        }).ToList();
    }
}
