csharp
public partial class AccountTaxGroup
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base class or another part of the partial class
        return Name;
    }

    // Example method to get the label of the selected Tribute AFIP Code
    public string GetTributeAfipCodeLabel()
    {
        return L10nArTributeAfipCode switch
        {
            L10nArTributeAfipCode.NationalTaxes => "01 - National Taxes",
            L10nArTributeAfipCode.ProvincialTaxes => "02 - Provincial Taxes",
            L10nArTributeAfipCode.MunicipalTaxes => "03 - Municipal Taxes",
            L10nArTributeAfipCode.InternalTaxes => "04 - Internal Taxes",
            L10nArTributeAfipCode.VatPerception => "06 - VAT perception",
            L10nArTributeAfipCode.IibbPerception => "07 - IIBB perception",
            L10nArTributeAfipCode.MunicipalTaxesPerceptions => "08 - Municipal Taxes Perceptions",
            L10nArTributeAfipCode.OtherPerceptions => "09 - Other Perceptions",
            L10nArTributeAfipCode.Others => "99 - Others",
            _ => "Unknown"
        };
    }

    // Example method to get the percentage of the selected VAT AFIP Code
    public decimal GetVatPercentage()
    {
        return L10nArVatAfipCode switch
        {
            L10nArVatAfipCode.ZeroPercent => 0m,
            L10nArVatAfipCode.TenPointFivePercent => 10.5m,
            L10nArVatAfipCode.TwentyOnePercent => 21m,
            L10nArVatAfipCode.TwentySevenPercent => 27m,
            L10nArVatAfipCode.FivePercent => 5m,
            L10nArVatAfipCode.TwoPointFivePercent => 2.5m,
            _ => 0m
        };
    }
}
