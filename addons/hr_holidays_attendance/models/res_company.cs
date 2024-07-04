csharp
public partial class ResCompany
{
    public void CheckExtraHoursTimeOff()
    {
        var extraHoursTimeOffType = Env.Ref("hr_holidays_attendance.holiday_status_extra_hours", false);
        if (extraHoursTimeOffType == null)
        {
            return;
        }

        var allCompanies = Env.Search<ResCompany>().ToList();

        // Unarchive time off type if the feature is enabled
        if (allCompanies.Any(company => company.HrAttendanceOvertime && !extraHoursTimeOffType.Active))
        {
            extraHoursTimeOffType.ToggleActive();
        }

        // Archive time off type if the feature is disabled for all companies
        if (allCompanies.All(company => !company.HrAttendanceOvertime) && extraHoursTimeOffType.Active)
        {
            extraHoursTimeOffType.ToggleActive();
        }
    }

    public override void Write(Dictionary<string, object> vals)
    {
        base.Write(vals);

        if (vals.ContainsKey("HrAttendanceOvertime"))
        {
            CheckExtraHoursTimeOff();
        }
    }
}
