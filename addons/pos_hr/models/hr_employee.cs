csharp
public partial class PosHr.HrEmployee
{
    public virtual void LoadPosData(dynamic data)
    {
        var configId = Env.Get("Pos.Config").Browse(data["pos.config"]["data"][0]["id"]);
        var domain = GetLoadPosDataDomain(data);
        var fields = GetLoadPosDataFields(configId);

        var employees = Env.Get("PosHr.HrEmployee").Search(domain);
        var managerIds = employees.Where(emp => configId.Get("GroupPosManagerId") in emp.Get("UserId").Get("GroupsId").Ids).Select(emp => emp.Id).ToList();

        var employeesBarcodePin = employees.GetBarcodesAndPinHashed();
        var bpPerEmployeeId = employeesBarcodePin.ToDictionary(bp => bp.Id, bp => bp);

        employees = employees.Read(fields).ToList();

        foreach (var employee in employees)
        {
            if (employee.Get("UserId") && managerIds.Contains(employee.Get("UserId").Id) || data["pos.config"]["data"][0]["advanced_employee_ids"].Contains(employee.Id))
            {
                employee["Roles"] = "Manager";
            }
            else
            {
                employee["Roles"] = "Cashier";
            }

            employee["Barcode"] = bpPerEmployeeId[employee.Id]["Barcode"];
            employee["Pin"] = bpPerEmployeeId[employee.Id]["Pin"];
        }

        var result = new {
            Data = employees,
            Fields = fields,
        };

        return result;
    }

    private List<string> GetLoadPosDataFields(dynamic configId)
    {
        return new List<string> { "Name", "UserId", "WorkContactId" };
    }

    private dynamic GetLoadPosDataDomain(dynamic data)
    {
        var configId = Env.Get("Pos.Config").Browse(data["pos.config"]["data"][0]["id"]);

        if (configId.Get("BasicEmployeeIds").Count > 0)
        {
            return new {
                And = new[] {
                    new {
                        Field = "Company",
                        Operator = "=",
                        Value = configId.Get("Company").Id,
                    },
                    new {
                        Or = new[] {
                            new {
                                Field = "UserId",
                                Operator = "=",
                                Value = Env.User.Id,
                            },
                            new {
                                Field = "Id",
                                Operator = "in",
                                Value = configId.Get("BasicEmployeeIds").Ids.Concat(configId.Get("AdvancedEmployeeIds").Ids).ToList(),
                            }
                        }
                    }
                }
            };
        }
        else
        {
            return new {
                Field = "Company",
                Operator = "=",
                Value = configId.Get("Company").Id,
            };
        }
    }

    public virtual List<dynamic> GetBarcodesAndPinHashed()
    {
        if (!Env.User.Get("GroupsId").Contains(Env.Get("PointOfSale.GroupPosUser")))
        {
            return new List<dynamic>();
        }

        var visibleEmpIds = this.Search(new { Field = "Id", Operator = "in", Value = this.Ids }).Ids;
        var employeesData = this.WithUser(Env.User).SearchRead(new { Field = "Id", Operator = "in", Value = visibleEmpIds }, new[] { "Barcode", "Pin" });

        foreach (var e in employeesData)
        {
            e["Barcode"] = (e["Barcode"] != null) ? System.Security.Cryptography.SHA1.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(e["Barcode"].ToString())) : null;
            e["Pin"] = (e["Pin"] != null) ? System.Security.Cryptography.SHA1.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(e["Pin"].ToString())) : null;
        }

        return employeesData;
    }

    public virtual void UnlinkExceptActivePosSession()
    {
        var configsWithEmployees = Env.Get("Pos.Config").WithUser(Env.User).Search(new { Field = "ModulePosHr", Operator = "=", Value = true }).Where(c => c.Get("CurrentSessionId") != null).ToList();
        var configsWithAllEmployees = configsWithEmployees.Where(c => !c.Get("BasicEmployeeIds") && !c.Get("AdvancedEmployeeIds")).ToList();
        var configsWithSpecificEmployees = configsWithEmployees.Where(c => (c.Get("BasicEmployeeIds") || c.Get("AdvancedEmployeeIds")) && this in c.Get("BasicEmployeeIds")).ToList();

        if (configsWithAllEmployees.Count > 0 || configsWithSpecificEmployees.Count > 0)
        {
            var error_msg = "You cannot delete an employee that may be used in an active PoS session, close the session(s) first: \n";

            foreach (var employee in this)
            {
                var configIds = configsWithAllEmployees.Concat(configsWithSpecificEmployees.Where(c => employee in c.Get("BasicEmployeeIds"))).ToList();

                if (configIds.Count > 0)
                {
                    error_msg += $"Employee: {employee.Get("Name")} - PoS Config(s): {string.Join(", ", configIds.Select(c => c.Get("Name")))} \n";
                }
            }

            throw new Exception(error_msg);
        }
    }
}
