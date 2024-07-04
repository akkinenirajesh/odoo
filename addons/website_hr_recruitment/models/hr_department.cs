csharp
public partial class Website.HrDepartment {
    public string ComputeDisplayName() {
        return Env.Ref("Website.HrDepartment").DisplayName;
    }
}
