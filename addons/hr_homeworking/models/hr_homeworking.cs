csharp
public partial class EmployeeLocation
{
    public string ToString()
    {
        // Example string representation
        return $"{Employee.Name} - {WorkLocation.Name} ({Date:d})";
    }

    private void _ComputeDayWeekString()
    {
        DayWeekString = Date.DayOfWeek.ToString();
    }
}
