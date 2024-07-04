csharp
public partial class ResPartner
{
    public virtual IEdiBuilder GetEdiBuilder()
    {
        if (UblCiiFormat == UblCiiFormat.PintMy)
        {
            return Env.Get<IAccountEdiXmlPintMy>();
        }
        return base.GetEdiBuilder();
    }

    public virtual void ComputeUblCiiFormat()
    {
        base.ComputeUblCiiFormat();
        if (Country?.Code == "MY")
        {
            UblCiiFormat = UblCiiFormat.PintMy;
        }
    }
}
