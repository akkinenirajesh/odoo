csharp
public partial class TeamMember
{
    public int GetAssignmentQuota(bool forceQuota = false)
    {
        int quota = (int)Math.Round(AssignmentMax / 30.0, MidpointRounding.AwayFromZero);
        if (forceQuota)
        {
            return quota;
        }
        return quota - LeadDayCount;
    }

    public void ComputeLeadDayCount()
    {
        DateTime dayDate = DateTime.Now.AddHours(-24);
        var dailyLeadsCounts = GetLeadFromDate(dayDate, true);

        LeadDayCount = dailyLeadsCounts.TryGetValue((User.Id, CrmTeam.Id), out int count) ? count : 0;
    }

    public void ComputeLeadMonthCount()
    {
        DateTime monthDate = DateTime.Now.AddDays(-30);
        var monthlyLeadsCounts = GetLeadFromDate(monthDate);

        LeadMonthCount = monthlyLeadsCounts.TryGetValue((User.Id, CrmTeam.Id), out int count) ? count : 0;
    }

    private Dictionary<(int, int), int> GetLeadFromDate(DateTime dateFrom, bool activeTest = false)
    {
        var leads = Env.Set<Crm.Lead>().WithContext(new { active_test = activeTest });
        var groups = leads.ReadGroup(
            domain: new[]
            {
                ("DateOpen", ">=", dateFrom),
                ("Team", "in", new[] { CrmTeam.Id }),
                ("User", "in", new[] { User.Id })
            },
            fields: new[] { "User", "Team" },
            groupBy: new[] { "User", "Team" },
            lazy: false
        );

        return groups.ToDictionary(
            g => ((int)g["User"], (int)g["Team"]),
            g => (int)g["__count"]
        );
    }

    public void ConstrainsAssignmentDomain()
    {
        try
        {
            var domain = Env.Eval(AssignmentDomain ?? "[]") as object[];
            if (domain != null && domain.Length > 0)
            {
                Env.Set<Crm.Lead>().Search(domain, limit: 1);
            }
        }
        catch (Exception)
        {
            throw new ValidationException($"Member assignment domain for user {User.Name} and team {CrmTeam.Name} is incorrectly formatted");
        }
    }
}
