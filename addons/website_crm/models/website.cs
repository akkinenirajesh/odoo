C#
public partial class Website {
    public object GetCrmDefaultTeamDomain() {
        if (!this.Env.User.HasGroup("crm.group_use_lead")) {
            return new { use_opportunities = true };
        }
        return new { use_leads = true };
    }
}
