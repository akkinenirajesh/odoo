csharp
public partial class CrmTeam {

    public virtual int Sequence { get; set; }
    public virtual bool Active { get; set; }
    public virtual ResCompany Company { get; set; }
    public virtual ResCurrency Currency { get; set; }
    public virtual ResUsers TeamLeader { get; set; }
    public virtual bool IsMembershipMulti { get; set; }
    public virtual ICollection<ResUsers> Members { get; set; }
    public virtual ICollection<ResCompany> MemberCompanyIds { get; set; }
    public virtual string MemberWarning { get; set; }
    public virtual ICollection<CrmTeamMember> CrmTeamMembers { get; set; }
    public virtual ICollection<CrmTeamMember> CrmTeamMembersAll { get; set; }
    public virtual int Color { get; set; }
    public virtual ICollection<ResUsers> FavoriteUsers { get; set; }
    public virtual bool IsFavorite { get; set; }
    public virtual string DashboardButtonName { get; set; }
    public virtual string DashboardGraphData { get; set; }

    public void ComputeIsMembershipMulti() {
        this.IsMembershipMulti = Env.Config.GetParam("sales_team.membership_multi", false);
    }

    public void ComputeMembers() {
        this.Members = this.CrmTeamMembers.Where(x => x.Active).Select(x => x.User).ToList();
    }

    public void InverseMembers() {
        var memberships = this.CrmTeamMembers;
        var usersCurrent = this.Members;
        var usersNew = usersCurrent.Except(memberships.Select(x => x.User)).ToList();

        // Add missing memberships
        Env.Create<CrmTeamMember>(usersNew.Select(x => new CrmTeamMember { CrmTeam = this, User = x }));

        // Activate or deactivate other memberships depending on members
        foreach (var membership in memberships) {
            membership.Active = usersCurrent.Contains(membership.User);
        }
    }

    public void ComputeMemberWarning() {
        if (this.IsMembershipMulti) {
            return;
        }
        var otherMemberships = Env.Search<CrmTeamMember>(x => x.CrmTeam != this && x.User.IsIn(this.Members)).ToList();
        if (otherMemberships.Count == 1) {
            this.MemberWarning = $"Adding {otherMemberships[0].User.Name} in this team would remove him/her from its current team {otherMemberships[0].CrmTeam.Name}.";
        } else if (otherMemberships.Count > 1) {
            this.MemberWarning = $"Adding {string.Join(", ", otherMemberships.Select(x => x.User.Name))} in this team would remove them from their current teams ({string.Join(", ", otherMemberships.Select(x => x.CrmTeam.Name))}).";
        }
        if (!string.IsNullOrEmpty(this.MemberWarning)) {
            this.MemberWarning += " To add a Salesperson into multiple Teams, activate the Multi-Team option in settings.";
        }
    }

    public void SearchMembers(string operator, object value) {
        // This method is not required.
        // The Search functionality is handled by the framework.
    }

    public void ComputeMemberCompanyIds() {
        this.MemberCompanyIds = this.Company != null ? new List<ResCompany> { this.Company } : Env.Search<ResCompany>().ToList();
    }

    public void ComputeIsFavorite() {
        this.IsFavorite = Env.User.IsIn(this.FavoriteUsers);
    }

    public void InverseIsFavorite() {
        // This method is not required.
        // The framework handles favorite relations.
    }

    public void ComputeDashboardButtonName() {
        this.DashboardButtonName = "Big Pretty Button :)";
    }

    public void ComputeDashboardGraph() {
        this.DashboardGraphData = Json.Serialize(GetDashboardGraphData());
    }

    public List<object> GetDashboardGraphData() {
        // TODO: Implement the logic for getting dashboard graph data.
        return new List<object>();
    }

    public List<ResUsers> GetDefaultFavoriteUserIds() {
        return new List<ResUsers> { Env.User };
    }

}
