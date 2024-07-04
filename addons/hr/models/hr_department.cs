csharp
public partial class Department
{
    public override string ToString()
    {
        return Env.Context.GetValueOrDefault("hierarchical_naming", true) ? CompleteName : Name;
    }

    public void ComputeCompleteName()
    {
        if (Parent != null)
        {
            CompleteName = $"{Parent.CompleteName} / {Name}";
        }
        else
        {
            CompleteName = Name;
        }
    }

    public void ComputeMasterDepartmentId()
    {
        if (!string.IsNullOrEmpty(ParentPath))
        {
            var masterDepartmentId = int.Parse(ParentPath.Split('/')[0]);
            MasterDepartment = Env.Get<Department>(masterDepartmentId);
        }
    }

    public void ComputeTotalEmployee()
    {
        var employeeCount = Env.Query<Employee>()
            .Where(e => e.Department == this)
            .Count();
        TotalEmployee = employeeCount;
    }

    public void ComputePlanCount()
    {
        var planCount = Env.Query<ActivityPlan>()
            .Where(p => p.Department == this)
            .Count();
        PlansCount = planCount;
    }

    public void CheckParentId()
    {
        if (HasCycle())
        {
            throw new ValidationException("You cannot create recursive departments.");
        }
    }

    public void UpdateEmployeeManager(Employee newManager)
    {
        var employees = Env.Query<Employee>()
            .Where(e => e.Id != newManager.Id && e.Department == this && e.Parent == Manager)
            .ToList();

        foreach (var employee in employees)
        {
            employee.Parent = newManager;
        }
    }

    public Dictionary<string, object> GetDepartmentHierarchy()
    {
        var hierarchy = new Dictionary<string, object>
        {
            ["parent"] = Parent != null ? new
            {
                id = Parent.Id,
                name = Parent.Name,
                employees = Parent.TotalEmployee
            } : null,
            ["self"] = new
            {
                id = Id,
                name = Name,
                employees = TotalEmployee
            },
            ["children"] = Children.Select(child => new
            {
                id = child.Id,
                name = child.Name,
                employees = child.TotalEmployee
            }).ToList()
        };

        return hierarchy;
    }
}
