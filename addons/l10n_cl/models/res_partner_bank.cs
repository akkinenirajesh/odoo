csharp
public partial class ResBank
{
    public string GetFiscalCountryCodes()
    {
        return string.Join(",", Env.Companies.Select(c => c.AccountFiscalCountry?.Code).Where(c => c != null));
    }

    public override string ToString()
    {
        return Name;
    }
}
