csharp
public partial class AnalyticLine
{
    public override string ToString()
    {
        return $"Analytic Line: {Date} - {Employee?.Name} - {Project?.Name}";
    }

    public void PopulateFactories()
    {
        var projectsGroups = Env.ProjectProject.ReadGroup(
            domain: new[] { ("Id", "in", Env.Registry.PopulatedModels["Project.Project"]) },
            groupBy: new[] { "Company" },
            aggregates: new[] { "Id:array_agg" }
        );

        var projectIds = new List<int>();
        var projectsPerCompany = new Dictionary<int, List<int>>();

        foreach (var group in projectsGroups)
        {
            var company = group.Company;
            var ids = group.IdArrayAgg;
            projectIds.AddRange(ids);
            projectsPerCompany[company.Id] = ids.ToList();
        }

        var tasksPerProject = Env.ProjectTask.ReadGroup(
            domain: new[]
            {
                ("Id", "in", Env.Registry.PopulatedModels["Project.Task"]),
                ("Project", "in", projectIds)
            },
            groupBy: new[] { "Project" },
            aggregates: new[] { "Id:array_agg" }
        ).ToDictionary(g => g.Project.Id, g => g.IdArrayAgg);

        var employeesPerCompany = Env.HrEmployee.ReadGroup(
            domain: new[] { ("Id", "in", Env.Registry.PopulatedModels["Hr.Employee"]) },
            groupBy: new[] { "Company" },
            aggregates: new[] { "Id:array_agg" }
        ).ToDictionary(g => g.Company.Id, g => g.IdArrayAgg);

        var companyIds = Env.Registry.PopulatedModels["Core.Company"]
            .Intersect(employeesPerCompany.Keys)
            .Intersect(projectsPerCompany.Keys)
            .ToList();

        // The rest of the population logic would go here, but it would need to be adapted
        // to work with the C# environment and random number generation utilities.
    }
}
