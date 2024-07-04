csharp
public partial class ResCompany {
    public List<Dictionary<string, object>> GetFiscalDates(List<Dictionary<string, object>> payload)
    {
        var companies = Env.Get("Res.Company").Browse(payload.Select(data => data["company_id"] ?? Env.Company.Id).ToList());
        var existingCompanies = companies.Exists();
        // prefetch both fields
        existingCompanies.Fetch("FiscalYearLastDay", "FiscalYearLastMonth");
        var results = new List<Dictionary<string, object>>();

        foreach (var (data, company) in payload.Zip(companies, (d, c) => (d, c)))
        {
            if (!existingCompanies.Contains(company))
            {
                results.Add(new Dictionary<string, object>() { { "start", false }, { "end", false } });
                continue;
            }
            var (start, end) = DateUtils.GetFiscalYear(
                DateTime.Parse(data["date"].ToString()),
                company.FiscalYearLastDay,
                company.FiscalYearLastMonth
            );
            results.Add(new Dictionary<string, object>() { { "start", start }, { "end", end } });
        }
        return results;
    }
}
