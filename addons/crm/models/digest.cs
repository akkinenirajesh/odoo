csharp
public partial class Digest
{
    public void _ComputeKpiCrmLeadCreatedValue()
    {
        if (!Env.User.HasGroup("sales_team.group_sale_salesman"))
        {
            throw new AccessException("Do not have access, skip this data for user's digest email");
        }

        _CalculateCompanyBasedKpi("Crm.Lead", "KpiCrmLeadCreatedValue");
    }

    public void _ComputeKpiCrmOpportunitiesWonValue()
    {
        if (!Env.User.HasGroup("sales_team.group_sale_salesman"))
        {
            throw new AccessException("Do not have access, skip this data for user's digest email");
        }

        _CalculateCompanyBasedKpi(
            "Crm.Lead",
            "KpiCrmOpportunitiesWonValue",
            dateField: "DateClosed",
            additionalDomain: new List<object> { new List<object> { "Type", "=", "opportunity" }, new List<object> { "Probability", "=", "100" } }
        );
    }

    public Dictionary<string, string> _ComputeKpisActions(Company company, User user)
    {
        var res = base._ComputeKpisActions(company, user);
        res["KpiCrmLeadCreated"] = $"crm.crm_lead_action_pipeline&menu_id={Env.Ref("crm.crm_menu_root").Id}";
        res["KpiCrmOpportunitiesWon"] = $"crm.crm_lead_action_pipeline&menu_id={Env.Ref("crm.crm_menu_root").Id}";
        
        if (user.HasGroup("crm.group_use_lead"))
        {
            res["KpiCrmLeadCreated"] = $"crm.crm_lead_all_leads&menu_id={Env.Ref("crm.crm_menu_root").Id}";
        }
        
        return res;
    }
}
