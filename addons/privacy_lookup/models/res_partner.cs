csharp
public partial class PrivacyLookup.ResPartner
{
    public Buvi.Action ActionPrivacyLookup()
    {
        var action = Env.Get<Buvi.IrActionsActWindow>()._ForXmlId("privacy_lookup.action_privacy_lookup_wizard");
        action.Context = new Dictionary<string, object>
        {
            { "default_email", this.Email },
            { "default_name", this.Name }
        };
        return action;
    }
}
