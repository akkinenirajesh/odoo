csharp
public partial class ResourceResource
{
    public string ComputeAvatar128()
    {
        var isHrUser = Env.User.HasGroup("HR.GroupHrUser");
        if (!isHrUser)
        {
            var publicEmployees = Env.Set<HR.EmployeePublic>()
                .WithContext(new { active_test = false })
                .Search(e => e.ResourceId == this.Id);

            var avatarPerEmployeeId = publicEmployees.ToDictionary(emp => emp.Id, emp => emp.Avatar128);

            if (this.Employee != null && this.Employee.Any())
            {
                return avatarPerEmployeeId[this.Employee.First().Id];
            }
        }
        else
        {
            if (this.Employee != null && this.Employee.Any())
            {
                return this.Employee.First().Avatar128;
            }
        }

        return null;
    }
}
