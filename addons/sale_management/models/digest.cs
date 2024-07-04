csharp
public partial class Sale.Digest
{
    public void ComputeKpiSaleTotalValue()
    {
        if (!Env.User.HasGroup("sales_team.group_sale_salesman_all_leads"))
        {
            throw new AccessError("Do not have access, skip this data for user's digest email");
        }
        this.KpiAllSaleTotalValue = Env.CalculateCompanyBasedKpi(
            "sale.report",
            "KpiAllSaleTotalValue",
            "date",
            new[] { ("state", "not in", new[] { "draft", "cancel", "sent" }) },
            "price_total"
        );
    }

    public Dictionary<string, string> ComputeKpisActions(Core.Company company, Core.User user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiAllSaleTotal"] = "sale.report_all_channels_sales_action&menu_id=%s" % Env.Ref("sale.sale_menu_root").Id;
        return res;
    }
}
