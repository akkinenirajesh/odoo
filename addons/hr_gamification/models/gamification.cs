csharp
public partial class GamificationBadgeUser
{
    public void CheckEmployeeRelatedUser()
    {
        if (this.EmployeeId != null && !this.UserId.WithContext(new { allowed_company_ids = Env.User.CompanyIds.Select(c => c.Id) }).EmployeeIds.Contains(this.EmployeeId))
        {
            throw new ValidationException("The selected employee does not correspond to the selected user.");
        }
    }

    public Dictionary<string, object> ActionOpenBadge()
    {
        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["res_model"] = "HrGamification.GamificationBadge",
            ["view_mode"] = "form",
            ["res_id"] = this.BadgeId.Id
        };
    }
}

public partial class GamificationBadge
{
    private void _ComputeGrantedEmployeesCount()
    {
        this.GrantedEmployeesCount = Env.Set<GamificationBadgeUser>().Search(new[]
        {
            ("BadgeId", "=", this.Id),
            ("EmployeeId", "!=", null)
        }).Count();
    }

    public Dictionary<string, object> GetGrantedEmployees()
    {
        var employeeIds = this.OwnerIds.Select(o => o.EmployeeId).Where(e => e != null).Select(e => e.Id).ToList();
        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["name"] = "Granted Employees",
            ["view_mode"] = "kanban,tree,form",
            ["res_model"] = "Hr.EmployeePublic",
            ["domain"] = new List<object> { new List<object> { "Id", "in", employeeIds } }
        };
    }
}
