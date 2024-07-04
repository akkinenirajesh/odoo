csharp
public partial class EmployeeBase
{
    public bool SearchFilterForExpense(string @operator, object value)
    {
        if (@operator != "=" || value == null)
        {
            throw new ArgumentException("Operation not supported");
        }

        var user = Env.User;
        var employee = user.Employee;

        if (user.HasGroups("Hr.GroupHrExpenseUser") || user.HasGroups("Account.GroupAccountUser"))
        {
            // Domain accepts everything
            return true;
        }
        else if (user.HasGroups("Hr.GroupHrExpenseTeamApprover") && user.EmployeeIds.Any())
        {
            return this.DepartmentId.ManagerId == employee.Id ||
                   this.ParentId == employee.Id ||
                   this.Id == employee.Id ||
                   this.ExpenseManagerId == user.Id;
        }
        else if (user.Employee != null)
        {
            return this.Id == employee.Id;
        }

        return false;
    }
}

public partial class Employee
{
    public IEnumerable<Core.User> GroupHrExpenseUserDomain()
    {
        var group = Env.Ref<Core.Group>("Hr.GroupHrExpenseTeamApprover", raiseIfNotFound: false);
        return group != null ? Env.Users.Where(u => u.Groups.Contains(group)) : Enumerable.Empty<Core.User>();
    }

    public void ComputeExpenseManager()
    {
        var previousManager = this.GetOrigin().ParentId?.UserId;
        var manager = this.ParentId?.UserId;

        if (manager != null && manager.HasGroup("Hr.GroupHrExpenseUser") &&
            (this.ExpenseManagerId == previousManager || this.ExpenseManagerId == null))
        {
            this.ExpenseManagerId = manager;
        }
        else if (this.ExpenseManagerId == null)
        {
            this.ExpenseManagerId = null;
        }
    }

    public override IEnumerable<string> GetUserM2oToEmptyOnArchivedEmployees()
    {
        var fields = base.GetUserM2oToEmptyOnArchivedEmployees().ToList();
        fields.Add("ExpenseManagerId");
        return fields;
    }
}

public partial class User
{
    public IEnumerable<string> SelfReadableFields
    {
        get
        {
            var fields = base.SelfReadableFields.ToList();
            fields.Add("ExpenseManagerId");
            return fields;
        }
    }
}
