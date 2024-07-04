C#
public partial class Company {
    public void Init() {
        var typeIdsRef = Env.Ref("hr_timesheet.internal_project_default_stage", false);
        var typeIds = typeIdsRef != null ? new List<object>() { new object[] { 4, typeIdsRef.Id } } : new List<object>();
        var companies = Search(new List<object>() { new object[] { "OR", new object[] { "InternalProjectId", "=", null }, new object[] { "LeaveTimesheetTaskId", "=", null } } });
        Dictionary<int, int> internalProjectsByCompanyDict = null;
        foreach (var company in companies) {
            using (var companyContext = new CompanyContext(company.Id)) {
                if (company.InternalProjectId == null) {
                    if (internalProjectsByCompanyDict == null) {
                        var internalProjectsByCompanyRead = Env.GetModel("Project.Project").SearchRead(new List<object>() {
                            new object[] { "Name", "=", "Internal" },
                            new object[] { "AllowTimesheets", "=", true },
                            new object[] { "CompanyId", "in", companies.Ids }
                        }, new List<string>() { "CompanyId", "Id" });
                        internalProjectsByCompanyDict = internalProjectsByCompanyRead.ToDictionary(res => (int)res["CompanyId"], res => (int)res["Id"]);
                    }
                    int projectId = internalProjectsByCompanyDict.GetValueOrDefault(company.Id, 0);
                    if (projectId == 0) {
                        projectId = Env.GetModel("Project.Project").Create(new Dictionary<string, object>() {
                            { "Name", "Internal" },
                            { "AllowTimesheets", true },
                            { "CompanyId", company.Id },
                            { "TypeIds", typeIds }
                        });
                    }
                    Write(new Dictionary<string, object>() {
                        { "InternalProjectId", projectId }
                    });
                }
                if (company.LeaveTimesheetTaskId == null) {
                    var task = Env.GetModel("Project.Task").Create(new Dictionary<string, object>() {
                        { "Name", "Time Off" },
                        { "ProjectId", company.InternalProjectId },
                        { "Active", true },
                        { "CompanyId", company.Id }
                    });
                    Write(new Dictionary<string, object>() {
                        { "LeaveTimesheetTaskId", task.Id }
                    });
                }
            }
        }
    }
    public List<object> CreateInternalProjectTask() {
        var projects = (List<object>)base.CreateInternalProjectTask();
        foreach (var project in projects) {
            var company = (Company)project;
            using (var companyContext = new CompanyContext(company.Id)) {
                if (company.LeaveTimesheetTaskId == null) {
                    var task = Env.GetModel("Project.Task").Create(new Dictionary<string, object>() {
                        { "Name", "Time Off" },
                        { "ProjectId", company.InternalProjectId },
                        { "Active", true },
                        { "CompanyId", company.Id }
                    });
                    company.Write(new Dictionary<string, object>() {
                        { "LeaveTimesheetTaskId", task.Id }
                    });
                }
            }
        }
        return projects;
    }
}
