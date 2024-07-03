csharp
public partial class Digest
{
    public void ComputeKpiAccountTotalRevenueValue()
    {
        if (!Env.User.HasGroup("account.group_account_invoice"))
        {
            throw new AccessError("Do not have access, skip this data for user's digest email");
        }

        var (start, end, companies) = GetKpiComputeParameters();

        var totalPerCompanies = Env.Set<AccountMoveLine>().Sudo().ReadGroup(
            groupBy: new[] { "Company" },
            aggregates: new[] { ("Balance", "sum") },
            domain: new[]
            {
                ("Company", "in", companies.Select(c => c.Id)),
                ("Date", ">", start),
                ("Date", "<=", end),
                ("Account.InternalGroup", "=", "income"),
                ("ParentState", "=", "posted")
            }
        );

        var company = Company ?? Env.Company;
        KpiAccountTotalRevenueValue = -totalPerCompanies.GetValueOrDefault(company, 0m);
    }

    public Dictionary<string, string> ComputeKpisActions(Company company, User user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiAccountTotalRevenue"] = $"account.action_move_out_invoice_type&menu_id={Env.Ref<Menu>("account.menu_finance").Id}";
        return res;
    }
}
