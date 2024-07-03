csharp
public partial class AnalyticLine
{
    private static readonly Dictionary<string, int> PopulateSizes = new Dictionary<string, int>
    {
        { "small", 100 },
        { "medium", 1000 },
        { "large", 10000000 }
    };

    public IEnumerable<AnalyticLine> Populate(string size)
    {
        if (!PopulateSizes.TryGetValue(size, out int count))
        {
            throw new ArgumentException("Invalid size specified", nameof(size));
        }

        var random = new Random();
        var accounts = Env.Get<Analytic.AnalyticAccount>().GetAll();
        var projectPlan = Env.Get<Account.AnalyticPlan>().GetProjectPlan();
        var otherPlans = Env.Get<Account.AnalyticPlan>().GetOtherPlans();

        for (int i = 0; i < count; i++)
        {
            var line = new AnalyticLine
            {
                Amount = (decimal)(random.NextDouble() * 1000),
                Name = $"Line {i + 1}",
                Account = accounts[random.Next(accounts.Count)]
            };

            // Assign random account for each plan
            foreach (var plan in projectPlan.Concat(otherPlans))
            {
                var planAccounts = accounts.Where(a => a.Plan == plan).ToList();
                if (planAccounts.Any())
                {
                    // Assuming there's a method to set the account for a specific plan
                    line.SetAccountForPlan(plan, planAccounts[random.Next(planAccounts.Count)]);
                }
            }

            yield return line;
        }
    }
}
