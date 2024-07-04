csharp
public partial class Project.Digest 
{
    public void ComputeProjectTaskOpenedValue()
    {
        if (!Env.User.IsInGroup("project.group_project_user"))
        {
            throw new AccessError("Do not have access, skip this data for user's digest email");
        }

        CalculateCompanyBasedKpi(
            "project.task",
            "KpiProjectTaskOpenedValue",
            new List<object[]> { new object[] { "stage_id.fold", "=", false }, new object[] { "project_id", "!=", false } });
    }

    public Dictionary<string, string> ComputeKpisActions(Company company, User user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiProjectTaskOpened"] = "project.open_view_project_all&menu_id=%s" % Env.Ref("project.menu_main_pm").Id;
        return res;
    }
}
