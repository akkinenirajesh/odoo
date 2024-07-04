csharp
public partial class AccountTax
{
    public override string ToString()
    {
        // Assuming there's a Name property in the base AccountTax class
        return Name;
    }

    public void UpdateSpanishTaxInfo(Account.ExemptReasonSpain exemptReason, Account.TaxTypeSpain taxType, bool bienInversion)
    {
        L10nEsExemptReason = exemptReason;
        L10nEsType = taxType;
        L10nEsBienInversion = bienInversion;

        // Assuming there's a Save method to persist changes
        Env.Save(this);
    }

    public bool IsSpanishExempt()
    {
        return L10nEsType == Account.TaxTypeSpain.Exento;
    }

    public bool IsSpanishInvestmentGood()
    {
        return L10nEsBienInversion;
    }
}
