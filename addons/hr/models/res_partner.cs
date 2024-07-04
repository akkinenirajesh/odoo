csharp
public partial class Partner
{
    public int EmployeesCount { get; set; }

    public void _ComputeEmployeesCount()
    {
        EmployeesCount = this.Sudo().EmployeeIds.Where(e => Env.Companies.Contains(e.CompanyId)).Count();
    }

    public object ActionOpenEmployees()
    {
        if (EmployeesCount > 1)
        {
            return new
            {
                name = "Related Employees",
                type = "ir.actions.act_window",
                res_model = "Hr.Employee",
                view_mode = "kanban",
                domain = new List<object>
                {
                    new List<object> { "Id", "in", EmployeeIds.Select(e => e.Id).ToList() },
                    new List<object> { "CompanyId", "in", Env.Companies.Select(c => c.Id).ToList() }
                }
            };
        }

        return new
        {
            name = "Employee",
            type = "ir.actions.act_window",
            res_model = "Hr.Employee",
            res_id = EmployeeIds.FirstOrDefault(e => Env.Companies.Contains(e.CompanyId))?.Id,
            view_mode = "form"
        };
    }

    public List<Dictionary<string, object>> _GetAllAddr()
    {
        var employeeId = Env.Get<HrEmployee>().Search(new List<object>
        {
            new List<object> { "Id", "in", EmployeeIds.Select(e => e.Id).ToList() }
        }, limit: 1).FirstOrDefault();

        if (employeeId == null)
        {
            return base._GetAllAddr();
        }

        var pstlAddr = new Dictionary<string, object>
        {
            { "contact_type", "employee" },
            { "street", employeeId.PrivateStreet },
            { "zip", employeeId.PrivateZip },
            { "city", employeeId.PrivateCity },
            { "country", employeeId.PrivateCountryId.Code }
        };

        return new List<Dictionary<string, object>> { pstlAddr }.Concat(base._GetAllAddr()).ToList();
    }
}
