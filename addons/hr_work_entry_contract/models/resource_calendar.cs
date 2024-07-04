csharp
public partial class ResourceCalendar
{
    public IEnumerable<ResourceCalendarAttendance> GetGlobalAttendances()
    {
        var superResult = base.GetGlobalAttendances();
        return superResult.Where(a => !a.WorkEntryType.IsLeave);
    }
}
