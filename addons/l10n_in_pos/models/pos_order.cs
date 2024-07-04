csharp
public partial class PosOrder
{
    public Dictionary<string, object> PrepareInvoiceVals()
    {
        var vals = base.PrepareInvoiceVals();

        if (this.SessionId.CompanyId.CountryId.Code == "IN")
        {
            var partner = this.PartnerId;
            var l10nInGstTreatment = partner.L10nInGstTreatment;

            if (string.IsNullOrEmpty(l10nInGstTreatment) && partner.CountryId != null && partner.CountryId.Code != "IN")
            {
                l10nInGstTreatment = "overseas";
            }

            if (string.IsNullOrEmpty(l10nInGstTreatment))
            {
                l10nInGstTreatment = !string.IsNullOrEmpty(partner.Vat) ? "regular" : "consumer";
            }

            vals["L10nInGstTreatment"] = l10nInGstTreatment;
        }

        return vals;
    }
}
