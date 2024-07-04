csharp
public partial class User
{
    public string[] SelfReadableFields => new[]
    {
        "LeaveManagerId",
        "ShowLeaves",
        "AllocationCount",
        "LeaveDateTo",
        "CurrentLeaveState",
        "IsAbsent",
        "AllocationRemainingDisplay",
        "AllocationDisplay",
        "HrIconDisplay"
    };

    public void ComputeImStatus()
    {
        base.ComputeImStatus();
        var onLeaveUserIds = GetOnLeaveIds();
        if (onLeaveUserIds.Contains(this.Id))
        {
            switch (this.ImStatus)
            {
                case "online":
                    this.ImStatus = "leave_online";
                    break;
                case "away":
                    this.ImStatus = "leave_away";
                    break;
                default:
                    this.ImStatus = "leave_offline";
                    break;
            }
        }
    }

    public List<int> GetOnLeaveIds(bool partner = false)
    {
        var now = DateTime.Now;
        var field = partner ? "PartnerId" : "Id";
        
        // Note: This is a placeholder for the actual SQL query execution
        // You'll need to implement the actual data access method here
        var query = $@"SELECT res_users.{field} FROM res_users
                       JOIN hr_leave ON hr_leave.UserId = res_users.Id
                       AND State = 'validate'
                       AND res_users.Active = true
                       AND DateFrom <= @now AND DateTo >= @now";

        // Execute the query and return the results
        // This is just a placeholder, replace with actual data access code
        return new List<int>();
    }

    public void CleanLeaveResponsibleUsers()
    {
        const string approverGroup = "HR.Holidays.Responsible";
        if (!this.HasGroup(approverGroup))
        {
            return;
        }

        // Note: This is a placeholder for the actual group by query
        // You'll need to implement the equivalent logic in C#
        var res = Env.Get<HR.Employee>().ReadGroup(
            new[] { ("LeaveManagerId", "in", this.Ids) },
            new[] { "LeaveManagerId" }
        );

        var responsibleIdsToRemove = new HashSet<int>(this.Ids);
        foreach (var group in res)
        {
            responsibleIdsToRemove.Remove(group.LeaveManagerId);
        }

        if (responsibleIdsToRemove.Any())
        {
            foreach (var userId in responsibleIdsToRemove)
            {
                var user = Env.Get<HR.User>().Browse(userId);
                user.Groups = user.Groups.Where(g => g.Id != Env.Ref(approverGroup).Id).ToList();
            }
        }
    }

    public static List<User> Create(List<Dictionary<string, object>> valsList)
    {
        var users = base.Create(valsList);
        users.CleanLeaveResponsibleUsers();
        return users;
    }
}
