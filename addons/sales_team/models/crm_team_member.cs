csharp
public partial class SalesTeam.CrmTeamMember
{
    // all the model methods are written here.
    public void _ConstrainsMembership()
    {
        // In mono membership mode: check CrmTeamId / UserId is unique for active
        // memberships. Inactive memberships can create duplicate pairs which is whyy
        // we don't use a SQL constraint. Include "this" in search in case we use create
        // multi with duplicated user / team pairs in it. Use an explicit active leaf
        // in domain as we may have an active_test in context that would break computation
        var existing = Env.Search<SalesTeam.CrmTeamMember>(new List<SearchCriteria>() {
            new SearchCriteria { Field = "CrmTeamId", Operator = SearchOperator.In, Value = this.CrmTeamId.Id.ToString() },
            new SearchCriteria { Field = "UserId", Operator = SearchOperator.In, Value = this.UserId.Id.ToString() },
            new SearchCriteria { Field = "Active", Operator = SearchOperator.Equal, Value = "true" }
        });

        var duplicates = new List<SalesTeam.CrmTeamMember>();

        var activeRecords = new Dictionary<int, int>();
        foreach (var membership in this)
        {
            if (membership.Active)
            {
                activeRecords[membership.UserId.Id] = membership.CrmTeamId.Id;
            }
        }

        foreach (var membership in this)
        {
            var potential = existing.Where(m => m.UserId == membership.UserId && m.CrmTeamId == membership.CrmTeamId && m.Id != membership.Id).ToList();
            if (potential == null || potential.Count > 1)
            {
                duplicates.AddRange(potential);
                continue;
            }

            if (activeRecords.ContainsKey(potential.FirstOrDefault().UserId.Id))
            {
                duplicates.AddRange(potential);
            }
            else
            {
                activeRecords[potential.FirstOrDefault().UserId.Id] = potential.FirstOrDefault().CrmTeamId.Id;
            }
        }

        if (duplicates.Count > 0)
        {
            throw new Exception(
                $"You are trying to create duplicate membership(s). We found that {string.Join(", ", duplicates.Select(m => $"{m.UserId.Name} ({m.CrmTeamId.Name})"))} already exist(s).");
        }
    }

    public void _ComputeUserInTeamsIds()
    {
        // Give users not to add in the currently chosen team to avoid duplicates.
        // In multi membership mode this field is empty as duplicates are allowed. 
        if (this.All(m => m.IsMembershipMulti))
        {
            this.UserInTeamsIds = Env.Search<Res.Users>(new List<SearchCriteria>());
        }
        else if (this.Id != 0)
        {
            this.UserInTeamsIds = Env.Search<SalesTeam.CrmTeamMember>(new List<SearchCriteria>() { new SearchCriteria { Field = "Id", Operator = SearchOperator.NotIn, Value = this.Id.ToString() } }).UserId;
        }
        else
        {
            this.UserInTeamsIds = Env.Search<SalesTeam.CrmTeamMember>(new List<SearchCriteria>()).UserId;
        }

        foreach (var member in this)
        {
            if (member.UserInTeamsIds != null)
            {
                member.UserInTeamsIds = member.UserInTeamsIds;
            }
            else if (member.CrmTeamId != null)
            {
                member.UserInTeamsIds = member.CrmTeamId.MemberIds;
            }
            else if (Env.Context.ContainsKey("default_crm_team_id"))
            {
                member.UserInTeamsIds = Env.Search<SalesTeam.CrmTeam>(new List<SearchCriteria>() { new SearchCriteria { Field = "Id", Operator = SearchOperator.Equal, Value = Env.Context["default_crm_team_id"].ToString() } }).MemberIds;
            }
            else
            {
                member.UserInTeamsIds = Env.Search<Res.Users>(new List<SearchCriteria>());
            }
        }
    }

    public void _ComputeUserCompanyIds()
    {
        var allCompanies = Env.Search<Res.Company>(new List<SearchCriteria>());
        foreach (var member in this)
        {
            member.UserCompanyIds = member.CrmTeamId.CompanyId ?? allCompanies;
        }
    }

    public void _ComputeIsMembershipMulti()
    {
        var multiEnabled = Env.GetParameter("sales_team.membership_multi", false);
        this.IsMembershipMulti = multiEnabled;
    }

    public void _ComputeMemberWarning()
    {
        // Display a warning message to warn user they are about to archive
        // other memberships. Only valid in mono-membership mode and take into
        // account only active memberships as we may keep several archived
        // memberships. 
        if (this.All(m => m.IsMembershipMulti))
        {
            this.MemberWarning = "";
        }
        else
        {
            var active = this.Where(m => m.Active).ToList();
            this.Where(m => !m.Active).ToList().ForEach(m => m.MemberWarning = "");

            if (active.Count == 0)
            {
                return;
            }

            var existing = Env.Search<SalesTeam.CrmTeamMember>(new List<SearchCriteria>() { new SearchCriteria { Field = "UserId", Operator = SearchOperator.In, Value = string.Join(",", active.Select(m => m.UserId.Id)) } });
            var userMapping = new Dictionary<Res.Users, List<SalesTeam.CrmTeam>>();
            foreach (var membership in existing)
            {
                if (!userMapping.ContainsKey(membership.UserId))
                {
                    userMapping.Add(membership.UserId, new List<SalesTeam.CrmTeam>());
                }
                userMapping[membership.UserId].Add(membership.CrmTeamId);
            }

            foreach (var member in active)
            {
                var teams = userMapping.ContainsKey(member.UserId) ? userMapping[member.UserId] : new List<SalesTeam.CrmTeam>();
                var remaining = teams.Except(new List<SalesTeam.CrmTeam>() { member.CrmTeamId, member._Origin.CrmTeamId }).ToList();
                if (remaining.Count > 0)
                {
                    member.MemberWarning = $"Adding {member.UserId.Name} in this team would remove him/her from its current teams {string.Join(", ", remaining.Select(t => t.Name))}.";
                }
                else
                {
                    member.MemberWarning = "";
                }
            }
        }
    }

    public void Create()
    {
        // Specific behavior implemented on create

        // * mono membership mode: other user memberships are automatically
        // archived (a warning already told it in form view);
        // * creating a membership already existing as archived: do nothing as
        // people can manage them from specific menu "Members";

        // Also remove autofollow on create. No need to follow team members
        // when creating them as chatter is mainly used for information purpose
        // (tracked fields).
        var isMembershipMulti = Env.GetParameter("sales_team.membership_multi", false);
        if (!isMembershipMulti)
        {
            _SynchronizeMemberships(new List<Dictionary<string, int>>() { new Dictionary<string, int>() { { "UserId", this.UserId.Id }, { "CrmTeamId", this.CrmTeamId.Id } } });
        }
        this.Create(new Dictionary<string, object>() { { "MailCreateNoSubscribe", true } });
    }

    public void Write(Dictionary<string, object> values)
    {
        // Specific behavior about active. If you change UserId / TeamId user
        // get warnings in form view and a raise in constraint check. We support
        // archive / activation of memberships that toggles other memberships. But
        // we do not support manual creation or update of UserId / TeamId. This
        // either works, either crashes). Indeed supporting it would lead to complex
        // code with low added value. Users should create or remove members, and
        // maybe archive / activate them. Updating manually memberships by
        // modifying UserId or TeamId is advanced and does not benefit from our
        // support. 
        var isMembershipMulti = Env.GetParameter("sales_team.membership_multi", false);
        if (!isMembershipMulti && values.ContainsKey("Active") && (bool)values["Active"])
        {
            _SynchronizeMemberships(this.Select(m => new Dictionary<string, int>() { { "UserId", m.UserId.Id }, { "CrmTeamId", m.CrmTeamId.Id } }).ToList());
        }
        this.Write(values);
    }

    private void _SynchronizeMemberships(List<Dictionary<string, int>> userTeamIds)
    {
        // Synchronize memberships: archive other memberships.

        // :param user_team_ids: list of pairs (user_id, crm_team_id)
        var existing = Env.Search<SalesTeam.CrmTeamMember>(new List<SearchCriteria>() {
            new SearchCriteria { Field = "Active", Operator = SearchOperator.Equal, Value = "true" },
            new SearchCriteria { Field = "UserId", Operator = SearchOperator.In, Value = string.Join(",", userTeamIds.Select(v => v["UserId"])) }
        });

        var userMemberships = new Dictionary<int, List<SalesTeam.CrmTeamMember>>();
        foreach (var membership in existing)
        {
            if (!userMemberships.ContainsKey(membership.UserId.Id))
            {
                userMemberships.Add(membership.UserId.Id, new List<SalesTeam.CrmTeamMember>());
            }
            userMemberships[membership.UserId.Id].Add(membership);
        }

        var existingToArchive = new List<SalesTeam.CrmTeamMember>();
        foreach (var values in userTeamIds)
        {
            existingToArchive.AddRange(userMemberships.ContainsKey(values["UserId"]) ? userMemberships[values["UserId"]].Where(m => m.CrmTeamId.Id != values["CrmTeamId"]).ToList() : new List<SalesTeam.CrmTeamMember>());
        }

        if (existingToArchive.Count > 0)
        {
            existingToArchive.ForEach(m => m.ActionArchive());
        }
    }
}
