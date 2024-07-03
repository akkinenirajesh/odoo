csharp
public partial class AnalyticLine
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeAutoAccount()
    {
        var plan = Env.Get<Account.AnalyticPlan>().Browse(Env.Context.GetValueOrDefault("analytic_plan_id"));
        if (plan != null)
        {
            AutoAccount = this[plan.ColumnName()];
        }
    }

    public void InverseAutoAccount()
    {
        this[AutoAccount.PlanId.ColumnName()] = AutoAccount;
    }

    public List<(string, string, object)> SearchAutoAccount(string @operator, object value)
    {
        var (projectPlan, otherPlans) = Env.Get<Account.AnalyticPlan>().GetAllPlans();
        var domains = new List<(string, string, object)>();

        foreach (var plan in projectPlan.Concat(otherPlans))
        {
            domains.Add((plan.ColumnName(), @operator, value));
        }

        return domains;
    }

    public List<string> GetPlanFieldNames()
    {
        var (projectPlan, otherPlans) = Env.Get<Account.AnalyticPlan>().GetAllPlans();
        return projectPlan.Concat(otherPlans)
            .Select(plan => plan.ColumnName())
            .Where(fname => this.GetType().GetProperty(fname) != null)
            .ToList();
    }

    public void CheckAccountId()
    {
        var fnames = GetPlanFieldNames();
        if (!fnames.Any(fname => this.GetType().GetProperty(fname).GetValue(this) != null))
        {
            throw new ValidationException("At least one analytic account must be set");
        }
    }
}
