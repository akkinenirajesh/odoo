csharp
public partial class WorkLocation
{
    public void UnlinkExceptUsedByEmployee()
    {
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        var domains = days.Select(day => (day, "in", this.Id)).ToList();
        
        var employeeUsesLocation = Env.Get<Hr.Employee>().SearchCount(domains, limit: 1);
        if (employeeUsesLocation > 0)
        {
            throw new UserException("You cannot delete locations that are being used by your employees");
        }
        
        var exceptionsUsingLocation = Env.Get<Hr.EmployeeLocation>().Search(new[] { ("WorkLocationId", "in", this.Id) });
        exceptionsUsingLocation.Unlink();
    }
}
