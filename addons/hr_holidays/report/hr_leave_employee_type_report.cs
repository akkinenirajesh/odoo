csharp
public partial class LeaveEmployeeTypeReport
{
    public Dictionary<string, object> ActionTimeOffAnalysis()
    {
        var domain = new List<object>();
        if (Env.Context.TryGetValue("active_ids", out var activeIds))
        {
            domain.Add(new[] { "Employee", "in", activeIds });
            domain.Add(new[] { "State", "!=", "Cancel" });
        }

        return new Dictionary<string, object>
        {
            { "name", "Time Off Analysis" },
            { "type", "ir.actions.act_window" },
            { "res_model", "HumanResources.LeaveEmployeeTypeReport" },
            { "view_mode", "pivot" },
            { "search_view_id", Env.Ref("hr_holidays.view_search_hr_holidays_employee_type_report").Id },
            { "domain", domain },
            { "context", new Dictionary<string, object>
                {
                    { "search_default_year", true },
                    { "search_default_company", true },
                    { "search_default_employee", true },
                    { "group_expand", true }
                }
            }
        };
    }
}
