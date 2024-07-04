c#
public partial class ResourceCalendarAttendance
{
    public void OnchangeHours()
    {
        this.HourFrom = Math.Min(this.HourFrom, 23.99);
        this.HourFrom = Math.Max(this.HourFrom, 0.0);
        this.HourTo = Math.Min(this.HourTo, 24);
        this.HourTo = Math.Max(this.HourTo, 0.0);

        this.HourTo = Math.Max(this.HourTo, this.HourFrom);
    }

    public DateTime GetWeekType(DateTime date)
    {
        // week_type is defined by
        //  * counting the number of days from January 1 of year 1
        //    (extrapolated to dates prior to the first adoption of the Gregorian calendar)
        //  * converted to week numbers and then the parity of this number is asserted.
        // It ensures that an even week number always follows an odd week number. With classical week number,
        // some years have 53 weeks. Therefore, two consecutive odd week number follow each other (53 --> 1).
        return DateTime.Now; // Implement this logic
    }

    public void ComputeDurationHours()
    {
        this.DurationHours = (this.HourTo - this.HourFrom) if (this.DayPeriod != "lunch") else 0;
    }

    public void ComputeDurationDays()
    {
        if (this.DayPeriod == "lunch")
        {
            this.DurationDays = 0;
        }
        else
        {
            this.DurationDays = 0.5 if (this.DurationHours <= Env.Ref<ResourceCalendar>(this.CalendarId).HoursPerDay * 3 / 4) else 1;
        }
    }

    public void ComputeDisplayName()
    {
        // this.DisplayName = ""; // Implement this logic
    }

    public ResourceCalendarAttendance CopyAttendanceVals()
    {
        var newAttendance = new ResourceCalendarAttendance();
        newAttendance.Name = this.Name;
        newAttendance.Dayofweek = this.Dayofweek;
        newAttendance.DateFrom = this.DateFrom;
        newAttendance.DateTo = this.DateTo;
        newAttendance.HourFrom = this.HourFrom;
        newAttendance.HourTo = this.HourTo;
        newAttendance.DayPeriod = this.DayPeriod;
        newAttendance.WeekType = this.WeekType;
        newAttendance.DisplayType = this.DisplayType;
        newAttendance.Sequence = this.Sequence;

        return newAttendance;
    }
}
