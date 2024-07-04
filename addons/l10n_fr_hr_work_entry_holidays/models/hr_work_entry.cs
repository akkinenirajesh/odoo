csharp
public partial class WorkEntry
{
    public bool FilterFrenchPartTimeEntries()
    {
        return Company.Country.Code == "FR" &&
               Employee.ResourceCalendar != Company.ResourceCalendar;
    }

    public bool MarkLeavesOutsideSchedule()
    {
        if (!FilterFrenchPartTimeEntries())
        {
            return base.MarkLeavesOutsideSchedule();
        }
        return false;
    }

    public double GetDuration()
    {
        if (!FilterFrenchPartTimeEntries())
        {
            return base.GetDuration();
        }

        if (base.GetDuration() == 0)
        {
            return (DateStop - DateStart).TotalHours;
        }

        return base.GetDuration();
    }
}
