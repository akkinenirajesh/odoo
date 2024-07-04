csharp
public partial class HrEmployee {

    public HrEmployee() {
    }

    public virtual Core.Company Company { get; set; }

    public virtual Core.Company GetDefaultCompany() {
        var projectCompanyId = Env.Context.Get("createProjectEmployeeMapping");
        if (projectCompanyId != null) {
            return Env.Ref<Core.Company>(projectCompanyId);
        }
        return null;
    }
}
