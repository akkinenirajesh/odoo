csharp
public partial class AnalyticAccount
{
    private const int SmallPopulateSize = 100;
    private const int MediumPopulateSize = 1000;
    private const int LargePopulateSize = 10000;

    public override string ToString()
    {
        return Name;
    }

    public List<(string FieldName, object Value)> PopulateFactories()
    {
        var projectPlan = SearchOrCreatePlan("Projects");
        var departmentPlan = SearchOrCreatePlan("Departments");

        return new List<(string, object)>
        {
            ("CompanyId", null),
            ("PlanId", ChooseRandomPlan(projectPlan.Id, departmentPlan.Id)),
            ("Name", $"Account {Env.Context.Counter}")
        };
    }

    private AnalyticPlan SearchOrCreatePlan(string name)
    {
        var plan = Env.Set<AnalyticPlan>().FirstOrDefault(p => p.Name == name);
        if (plan == null)
        {
            plan = Env.Set<AnalyticPlan>().Create(new { Name = name });
        }
        return plan;
    }

    private int ChooseRandomPlan(int projectPlanId, int departmentPlanId)
    {
        // Implement logic to choose a random plan based on the given probabilities
        // 99% chance for project plan, 1% chance for department plan
        // This is a simplified version and might need adjustment based on your random number generation method
        return Env.Random.NextDouble() < 0.99 ? projectPlanId : departmentPlanId;
    }
}
