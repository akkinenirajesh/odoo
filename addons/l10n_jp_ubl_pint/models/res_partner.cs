csharp
public partial class ResPartner
{
    public virtual Partner.UblCiiFormat GetEdiBuilder()
    {
        if (this.UblCiiFormat == Partner.UblCiiFormat.PintJp)
        {
            return Env.Get<Partner.UblCiiFormat>("Account.Edi.Xml.PintJp");
        }
        return base.GetEdiBuilder();
    }

    public virtual void ComputeUblCiiFormat()
    {
        base.ComputeUblCiiFormat();
        if (this.Country?.Code == "JP")
        {
            this.UblCiiFormat = Partner.UblCiiFormat.PintJp;
        }
    }
}
