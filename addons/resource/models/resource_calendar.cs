C#
public partial class ResourceCalendar
{
    // ... other methods ...
    public void SwitchCalendarType() {
        if (!this.CalendarIn2WeeksMode) {
            Env.Execute("resource.resourcecalendar", "Unlink", new { ids = this.Id });
            Env.Execute("resource.resourcecalendar", "Create", new { 
                calendarId = this.Id,
                name = "First week",
                dayOfWeek = 0,
                sequence = 0,
                hourFrom = 0,
                dayPeriod = "morning",
                weekType = "First",
                hourTo = 0,
                displayType = "line_section",
            });
            Env.Execute("resource.resourcecalendar", "Create", new {
                calendarId = this.Id,
                name = "Second week",
                dayOfWeek = 0,
                sequence = 25,
                hourFrom = 0,
                dayPeriod = "morning",
                weekType = "Second",
                hourTo = 0,
                displayType = "line_section",
            });

            this.CalendarIn2WeeksMode = true;
            var defaultAttendance = Env.Execute("resource.resourcecalendar", "DefaultGet", new { fields = "WorkingTime" })["WorkingTime"];
            foreach (var att in defaultAttendance) {
                att[2]["weekType"] = "First";
                att[2]["sequence"] = att[2]["sequence"] + 1;
            }
            Env.Execute("resource.resourcecalendar", "Write", new {
                ids = this.Id,
                WorkingTime = defaultAttendance
            });
            foreach (var att in defaultAttendance) {
                att[2]["weekType"] = "Second";
                att[2]["sequence"] = att[2]["sequence"] + 26;
            }
            Env.Execute("resource.resourcecalendar", "Write", new {
                ids = this.Id,
                WorkingTime = defaultAttendance
            });
        } else {
            this.CalendarIn2WeeksMode = false;
            Env.Execute("resource.resourcecalendar", "Unlink", new { ids = this.Id });
            var defaultAttendance = Env.Execute("resource.resourcecalendar", "DefaultGet", new { fields = "WorkingTime" })["WorkingTime"];
            Env.Execute("resource.resourcecalendar", "Write", new {
                ids = this.Id,
                WorkingTime = defaultAttendance
            });
        }
    }
}
