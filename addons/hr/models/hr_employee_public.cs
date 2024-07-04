csharp
public partial class EmployeePublic
{
    public bool ComputeIsManager()
    {
        var allReports = Env.Search<HR.EmployeePublic>(e => e.Id.IsChildOf(Env.User.Employee.Id));
        return allReports.Contains(this);
    }

    public IEnumerable<string> GetManagerOnlyFields()
    {
        return new List<string>();
    }

    public void ComputeManagerOnlyFields()
    {
        var managerFields = GetManagerOnlyFields();
        if (IsManager)
        {
            var employeeSudo = Employee.Sudo();
            foreach (var field in managerFields)
            {
                this[field] = employeeSudo[field];
            }
        }
        else
        {
            foreach (var field in managerFields)
            {
                this[field] = null;
            }
        }
    }

    public IEnumerable<long> SearchEmployee(string @operator, object value)
    {
        return Env.Search<HR.EmployeePublic>(e => e.Id.Compare(@operator, value)).Select(e => e.Id);
    }

    public HR.Employee ComputeEmployee()
    {
        return Env.Find<HR.Employee>(Id);
    }

    public override string ToString()
    {
        return Name;
    }
}
