csharp
public partial class Digest {
    public void ComputeKpiPosTotalValue() {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user")) {
            throw new AccessError("Do not have access, skip this data for user's digest email");
        }

        CalculateCompanyBasedKpi(
            "pos.order",
            "KpiPosTotalValue",
            "date_order",
            new List<string>() { "state", "not in", "draft", "cancel", "invoiced" },
            "amount_total"
        );
    }

    public Dictionary<string, string> ComputeKpisActions(Company company, User user) {
        var res = base.ComputeKpisActions(company, user);
        res["KpiPosTotal"] = "point_of_sale.action_pos_sale_graph&menu_id=%s" + Env.Ref("point_of_sale.menu_point_root").Id;
        return res;
    }

    private void CalculateCompanyBasedKpi(string modelName, string kpiFieldName, string dateFieldName, List<string> additionalDomain, string sumField) {
        // Implementation for CalculateCompanyBasedKpi
    }
}
