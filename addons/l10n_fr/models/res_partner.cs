csharp
public partial class ResPartner
{
    public string DeduceCountryCode()
    {
        if (!string.IsNullOrEmpty(this.Siret))
        {
            return "FR";
        }
        return base.DeduceCountryCode();
    }

    public List<string> PeppolEasEndpointDepends()
    {
        var baseDepends = base.PeppolEasEndpointDepends();
        baseDepends.Add("Siret");
        return baseDepends;
    }
}
