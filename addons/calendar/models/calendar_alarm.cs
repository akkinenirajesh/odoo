csharp
public partial class CalendarAlarm
{
    private static readonly Dictionary<string, string> _intervalSelection = new Dictionary<string, string>
    {
        {"Minutes", "Minutes"},
        {"Hours", "Hours"},
        {"Days", "Days"}
    };

    public void ComputeDurationMinutes()
    {
        switch (Interval)
        {
            case IntervalType.Minutes:
                DurationMinutes = Duration;
                break;
            case IntervalType.Hours:
                DurationMinutes = Duration * 60;
                break;
            case IntervalType.Days:
                DurationMinutes = Duration * 60 * 24;
                break;
            default:
                DurationMinutes = 0;
                break;
        }
    }

    public void ComputeMailTemplate()
    {
        if (AlarmType == AlarmType.Email && MailTemplate == null)
        {
            MailTemplate = Env.Ref<Mail.MailTemplate>("calendar.calendar_template_meeting_reminder");
        }
        else if (AlarmType != AlarmType.Email || MailTemplate == null)
        {
            MailTemplate = null;
        }
    }

    public void OnChangeDurationInterval()
    {
        string displayInterval = _intervalSelection.TryGetValue(Interval.ToString(), out var interval) ? interval : string.Empty;
        string displayAlarmType = AlarmType.ToString();
        Name = $"{displayAlarmType} - {Duration} {displayInterval}";
    }

    public override string ToString()
    {
        return Name;
    }
}
