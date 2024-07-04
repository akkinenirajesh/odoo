csharp
public partial class Attendance
{
    public List<object> GetOvertimeLeaveDomain()
    {
        var domain = base.GetOvertimeLeaveDomain();
        return And(domain, new List<object>
        {
            "|",
            new List<object> { "HolidayId.HolidayStatusId.TimeType", "=", "leave" },
            new List<object> { "ResourceId", "=", false }
        });
    }

    private List<object> And(List<object> domain1, List<object> domain2)
    {
        // Implement the AND logic here
        // This is a simplified representation and may need to be adjusted based on your actual domain structure
        var result = new List<object>(domain1);
        result.AddRange(domain2);
        return result;
    }
}
