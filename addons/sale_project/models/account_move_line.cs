C#
public partial class AccountMoveLine {
    public void ComputeAnalyticDistribution() {
        var projectAmls = this.Where(aml => aml.AnalyticDistribution != null && aml.SaleLineIds.Any(sl => sl.Project != null));
        Env.CallMethod("Account.AccountMoveLine", "_compute_analytic_distribution", this - projectAmls);

        var project = Env.Context.Get("project_id");
        if (project != null) {
            var analyticAccount = Env.Get("Project.Project").Browse(project).AnalyticAccount;
            foreach (var line in this) {
                line.AnalyticDistribution = new Dictionary<int, int>() { { analyticAccount.Id, 100 } };
            }
        }
    }
}
