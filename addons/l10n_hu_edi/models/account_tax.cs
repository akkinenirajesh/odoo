csharp
public partial class AccountTax
{
    private static readonly Dictionary<L10nHuTaxType, string> DefaultTaxReasons = new Dictionary<L10nHuTaxType, string>
    {
        { L10nHuTaxType.AAM, "AAM Tax exempt" },
        { L10nHuTaxType.TAM, "TAM Exempt property" },
        { L10nHuTaxType.KBAET, "KBAET sale to EU - VAT tv.§ 89." },
        { L10nHuTaxType.KBAUK, "KBAUK New means of transport within the EU - VAT tv.§ 89.§(2)" },
        { L10nHuTaxType.EAM, "EAM Product export to 3rd country - VAT tv.98-109.§" },
        { L10nHuTaxType.NAM, "NAM other export transaction VAT law § 110-118" },
        { L10nHuTaxType.ATK, "ATK Outside the scope of VAT - VAT tv.2-3.§" },
        { L10nHuTaxType.EUFAD37, "EUFAD37 § 37 (1) Reverse VAT in another EU country" },
        { L10nHuTaxType.EUFADE, "EUFADE Reverse charge of VAT in another EU country not VAT tv. § 37 (1)" },
        { L10nHuTaxType.EUE, "EUE Sales made in a 2nd EU country" },
        { L10nHuTaxType.HO, "HO Service to 3rd country" }
    };

    public void ComputeL10nHuTaxReason()
    {
        if (DefaultTaxReasons.TryGetValue(this.L10nHuTaxType, out string reason))
        {
            this.L10nHuTaxReason = reason;
        }
        else
        {
            this.L10nHuTaxReason = null;
        }
    }
}
