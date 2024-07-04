csharp
public partial class ResPartner
{
    public List<Hr.Employee> GetWorklocation(DateTime startDate, DateTime endDate)
    {
        var employees = Env.Search<Hr.Employee>(new[]
        {
            ("WorkContactId", "in", new[] { Id }),
            ("CompanyId", "=", Env.Company.Id)
        });

        return employees.GetWorklocation(startDate, endDate);
    }
}
