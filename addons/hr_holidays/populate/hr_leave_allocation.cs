csharp
public partial class HolidaysAllocation
{
    private static readonly Dictionary<string, int> PopulateSizes = new Dictionary<string, int>
    {
        { "small", 100 },
        { "medium", 800 },
        { "large", 10000 }
    };

    private static readonly string[] PopulateDependencies = { "HrHolidays.Employee", "HrHolidays.LeaveType" };

    public List<(string, Func<object>)> PopulateFactories()
    {
        var employeeIds = Env.Registry.PopulatedModels["HrHolidays.Employee"];
        var hrLeaveTypeIds = Env.Get<HrHolidays.LeaveType>()
            .Browse(Env.Registry.PopulatedModels["HrHolidays.LeaveType"])
            .Where(lt => lt.RequiresAllocation == "yes")
            .Select(lt => lt.Id)
            .ToList();

        return new List<(string, Func<object>)>
        {
            ("HolidayStatusId", () => Populate.Randomize(hrLeaveTypeIds)),
            ("EmployeeId", () => Populate.Randomize(employeeIds))
        };
    }
}
