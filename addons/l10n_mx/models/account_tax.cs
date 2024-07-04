csharp
public partial class AccountTax
{
    public void ComputeL10nMxTaxType()
    {
        if (Country?.Code == "MX")
        {
            L10nMxTaxType = L10nMxTaxType.iva;
        }
        else
        {
            L10nMxTaxType = null;
        }
    }

    public override string ToString()
    {
        // Implement a meaningful string representation of AccountTax
        return $"AccountTax: {L10nMxFactorType} - {L10nMxTaxType}";
    }
}
