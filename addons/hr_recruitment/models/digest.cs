csharp
public partial class Digest
{
    public void ComputeKpiHrRecruitmentNewColleaguesValue()
    {
        if (!Env.User.HasGroup("hr_recruitment.group_hr_recruitment_user"))
        {
            throw new AccessException("Do not have access, skip this data for user's digest email");
        }

        CalculateCompanyBasedKpi(
            "HrRecruitment.Employee",
            "KpiHrRecruitmentNewColleaguesValue"
        );
    }

    public Dictionary<string, string> ComputeKpisActions(Core.Company company, Core.User user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiHrRecruitmentNewColleagues"] = $"hr.open_view_employee_list_my&menu_id={Env.Ref("hr.menu_hr_root").Id}";
        return res;
    }

    private void CalculateCompanyBasedKpi(string modelName, string fieldName)
    {
        // Implementation of _calculate_company_based_kpi method
        // This would depend on how you want to calculate the KPI in your C# environment
    }
}
