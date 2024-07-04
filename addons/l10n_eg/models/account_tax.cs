csharp
public partial class AccountTax
{
    public override string ToString()
    {
        // Assuming there's a Name property in the base AccountTax class
        return $"{Name} - {L10nEgEtaCode}";
    }

    public string GetEtaCodeDescription()
    {
        return Env.Ref<Account.EtaTaxCode>().GetOptionLabel(L10nEgEtaCode);
    }
}
