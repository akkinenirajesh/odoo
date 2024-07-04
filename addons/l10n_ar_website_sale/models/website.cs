csharp
public partial class Website
{
    public bool DisplayPartnerB2bFields()
    {
        // Argentinean localization must always display b2b fields
        return Env.Company.Country.Code == "AR" || base.DisplayPartnerB2bFields();
    }
}
