csharp
public partial class AccountTax
{
    public void ComputeL10nArWithholdingPaymentType()
    {
        if (!L10nArWithholdingPaymentType.HasValue || TypeTaxUse != "none" || CountryCode != "AR")
        {
            L10nArWithholdingPaymentType = null;
        }
    }
}
