csharp
public partial class SaleOrder
{
    public void ComputeL10nInGstTreatment()
    {
        // Set default value as null so no error occurs for this field.
        this.L10nInGstTreatment = null;

        if (this.CountryCode == "IN")
        {
            var l10nInGstTreatment = this.Partner.L10nInGstTreatment;

            if (l10nInGstTreatment == null && this.Partner.Country != null && this.Partner.Country.Code != "IN")
            {
                l10nInGstTreatment = GstTreatment.Overseas;
            }

            if (l10nInGstTreatment == null)
            {
                l10nInGstTreatment = !string.IsNullOrEmpty(this.Partner.Vat) ? GstTreatment.Regular : GstTreatment.Consumer;
            }

            this.L10nInGstTreatment = l10nInGstTreatment;
        }
    }

    public Dictionary<string, object> PrepareInvoice()
    {
        var invoiceVals = base.PrepareInvoice();

        if (this.CountryCode == "IN")
        {
            invoiceVals["L10nInResellerPartner"] = this.L10nInResellerPartner;
            invoiceVals["L10nInGstTreatment"] = this.L10nInGstTreatment;
        }

        return invoiceVals;
    }
}
