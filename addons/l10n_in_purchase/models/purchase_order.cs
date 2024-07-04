csharp
public partial class PurchaseOrder
{
    public Purchase.GstTreatment ComputeL10nInGstTreatment()
    {
        // Set default value as null (equivalent to False in Python)
        Purchase.GstTreatment l10nInGstTreatment = null;

        if (this.CountryCode == "IN")
        {
            l10nInGstTreatment = this.Partner.L10nInGstTreatment;

            if (l10nInGstTreatment == null && this.Partner.Country != null && this.Partner.Country.Code != "IN")
            {
                l10nInGstTreatment = Purchase.GstTreatment.Overseas;
            }

            if (l10nInGstTreatment == null)
            {
                l10nInGstTreatment = !string.IsNullOrEmpty(this.Partner.Vat) 
                    ? Purchase.GstTreatment.Regular 
                    : Purchase.GstTreatment.Consumer;
            }
        }

        return l10nInGstTreatment;
    }
}
