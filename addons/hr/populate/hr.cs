csharp
public partial class Department
{
    public override string ToString()
    {
        return Name;
    }

    public void SetParentDepartments(List<Department> departments)
    {
        var rand = new Random("hr.department+parent_generator");
        var parentIds = departments.Where(d => rand.NextDouble() > 0.3).Select(d => d.Id).ToList();

        var parentChildren = new Dictionary<long, List<Department>>();
        foreach (var dept in departments)
        {
            var parent = rand.Next(parentIds);
            if (parent < dept.Id)
            {
                if (!parentChildren.ContainsKey(parent))
                    parentChildren[parent] = new List<Department>();
                parentChildren[parent].Add(dept);
            }
        }

        foreach (var pair in parentChildren)
        {
            foreach (var child in pair.Value)
            {
                child.ParentId = Env.Get<Department>(pair.Key);
            }
        }
    }
}

public partial class Job
{
    public override string ToString()
    {
        return Name;
    }
}

public partial class WorkLocation
{
    public override string ToString()
    {
        return Name;
    }
}

public partial class EmployeeCategory
{
    public override string ToString()
    {
        return Name;
    }
}

public partial class Employee
{
    public override string ToString()
    {
        return Name;
    }

    public void SetManager(List<Employee> employees)
    {
        var managerIds = new Dictionary<long, List<long>>();
        var rand = new Random("hr.employee+manager_generator");

        foreach (var employee in employees)
        {
            if (rand.NextDouble() >= 0.85 || !managerIds.ContainsKey(employee.CompanyId.Id))
            {
                if (!managerIds.ContainsKey(employee.CompanyId.Id))
                    managerIds[employee.CompanyId.Id] = new List<long>();
                managerIds[employee.CompanyId.Id].Add(employee.Id);
            }
        }

        foreach (var employee in employees)
        {
            var managerId = managerIds[employee.CompanyId.Id][rand.Next(managerIds[employee.CompanyId.Id].Count)];
            if (managerId != employee.Id)
            {
                employee.ParentId = Env.Get<Employee>(managerId);
            }
        }
    }
}
