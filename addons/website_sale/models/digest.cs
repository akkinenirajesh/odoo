csharp
public partial class Website.Digest
{
    public void ComputeKpiWebsiteSaleTotalValue()
    {
        if (!Env.User.HasGroup("sales_team.group_sale_salesman_all_leads"))
        {
            throw new AccessError("Do not have access, skip this data for user's digest email");
        }

        CalculateCompanyBasedKpi(
            "sale.report",
            "KpiWebsiteSaleTotalValue",
            "date",
            new List<object> { new object[] { "state", "not in", new List<object> { "draft", "cancel", "sent" } }, new object[] { "website_id", "!=", false } },
            "price_subtotal");
    }

    public Dictionary<string, string> ComputeKpisActions(Core.Company company, Res.Users user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiWebsiteSaleTotal"] = "website.backend_dashboard&menu_id=%s" % Env.Ref("website.menu_website_configuration").Id;
        return res;
    }
}
