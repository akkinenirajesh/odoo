csharp
public partial class ResPartner
{
    public void ComputeUblCiiFormat()
    {
        if (this.Country.Code == "RO")
        {
            this.UblCiiFormat = "ciusro";
        }
    }

    public AccountEdiXmlUblRo GetEdiBuilder()
    {
        if (this.UblCiiFormat == "ciusro")
        {
            return Env.Get<AccountEdiXmlUblRo>();
        }
        return null;
    }
}
