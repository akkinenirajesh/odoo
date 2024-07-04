C#
public partial class MrpWorkcenter 
{
    public void ComputeCostsHourAccountIds()
    {
        var record = this;
        record.CostsHourAccountIds = record.AnalyticDistribution.Count > 0
            ? Env.Instance.GetModel("Account.AnalyticAccount").Browse(record.AnalyticDistribution.Select(x => int.Parse(x)).ToList())
            : Env.Instance.GetModel("Account.AnalyticAccount").Browse(new List<int>());
    }

    public void CheckAnalytic()
    {
        var record = this;
        record.ValidateDistribution(new Dictionary<string, object> {
            { "companyId", record.Company.Id }
        });
    }

    private void ValidateDistribution(Dictionary<string, object> kwargs)
    {
        // Implementation for ValidateDistribution
    }
}
