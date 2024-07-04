csharp
public partial class ResourceResCompany {
    public void InitDataResourceCalendar() {
        var companies = Env.Model("Resource.ResCompany").Search(new List<object> { new Dictionary<string, object> { { "ResourceCalendarId", false } } });
        foreach (var company in companies) {
            company.CreateResourceCalendar();
        }
    }

    public void CreateResourceCalendar() {
        var valsList = new List<Dictionary<string, object>>();
        foreach (var company in Env.Model("Resource.ResCompany").Search(new List<object> { this })) {
            valsList.Add(company.PrepareResourceCalendarValues());
        }
        var resourceCalendars = Env.Model("Resource.ResourceCalendar").Create(valsList);
        foreach (var company in Env.Model("Resource.ResCompany").Search(new List<object> { this })) {
            company.ResourceCalendarId = (long)resourceCalendars[0].Id;
        }
    }

    public Dictionary<string, object> PrepareResourceCalendarValues() {
        return new Dictionary<string, object> {
            { "Name", "Standard 40 hours/week" },
            { "CompanyId", this.Id }
        };
    }

    public void Create(List<Dictionary<string, object>> valsList) {
        var companies = Env.Model("Resource.ResCompany").Create(valsList);
        var companiesWithoutCalendar = companies.Where(c => !c.ResourceCalendarId).ToList();
        foreach (var company in companiesWithoutCalendar) {
            company.CreateResourceCalendar();
        }
        foreach (var company in companies) {
            if (company.ResourceCalendarId != null && company.ResourceCalendarId != 0 && !company.ResourceCalendarId.HasCompany) {
                company.ResourceCalendarId.CompanyId = company.Id;
            }
        }
    }
}
