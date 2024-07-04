csharp
public partial class User 
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeEmployeeCount()
    {
        EmployeeCount = EmployeeIds.Count();
    }

    public void ComputeCanEdit()
    {
        bool canEdit = Env.Config.GetParam("hr.hr_employee_self_edit") || Env.User.HasGroup("Hr.GroupHrUser");
        CanEdit = canEdit;
    }

    public void ComputeIsSystem()
    {
        IsSystem = Env.User.IsSystem();
    }

    public void ComputeCompanyEmployee()
    {
        EmployeeId = EmployeeIds.FirstOrDefault(e => e.CompanyId == Env.Company);
    }

    public IEnumerable<int> SearchCompanyEmployee(string op, object value)
    {
        return Env.Set<Hr.Employee>().Search(new[] { ("UserId", "=", Id) }).Select(e => e.Id);
    }

    public void ActionCreateEmployee()
    {
        if (!CompanyIds.Contains(Env.Company))
        {
            throw new AccessException($"You are not allowed to create an employee because the user does not have access rights for {Env.Company.Name}");
        }

        var employee = new Hr.Employee
        {
            Name = Name,
            CompanyId = Env.Company.Id,
        };
        employee.SyncUser(this);
        employee.Save();
    }

    public object ActionOpenEmployees()
    {
        var employees = EmployeeIds;
        var model = Env.User.HasGroup("Hr.GroupHrUser") ? "Hr.Employee" : "Hr.EmployeePublic";

        if (employees.Count() > 1)
        {
            return new
            {
                Name = "Related Employees",
                Type = "Ir.Actions.Act_Window",
                ResModel = model,
                ViewMode = "kanban,tree,form",
                Domain = new object[] { new object[] { "Id", "in", employees.Select(e => e.Id).ToArray() } }
            };
        }

        return new
        {
            Name = "Employee",
            Type = "Ir.Actions.Act_Window",
            ResModel = model,
            ResId = employees.First().Id,
            ViewMode = "form"
        };
    }
}
