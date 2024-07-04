csharp
public partial class ResUsers {

    public void ComputeCrmTeamIds() {
        this.CrmTeamIds = this.CrmTeamMemberIds.Select(x => x.CrmTeamId).ToList();
    }

    public List<object> SearchCrmTeamIds(string operator, object value) {
        return this.Env.Search("CrmTeamMemberIds.CrmTeamId", operator, value);
    }

    public void ComputeSaleTeamId() {
        if (this.CrmTeamMemberIds.Count == 0) {
            this.SaleTeamId = null;
        } else {
            var sortedMemberships = this.CrmTeamMemberIds.OrderBy(x => x.CreateDate);
            this.SaleTeamId = sortedMemberships.FirstOrDefault().CrmTeamId;
        }
    }
}
