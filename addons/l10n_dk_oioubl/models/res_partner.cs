csharp
public partial class ResPartner
{
    public IEdiBuilder GetEdiBuilder()
    {
        if (UblCiiFormat == UblCiiFormatSelection.oioubl_201)
        {
            return Env.Get<IAccountEdiXmlOioubl201>();
        }
        return base.GetEdiBuilder();
    }

    public void ComputeUblCiiFormat()
    {
        base.ComputeUblCiiFormat();
        if (CountryCode == "DK")
        {
            UblCiiFormat = UblCiiFormatSelection.oioubl_201;
        }
    }
}
