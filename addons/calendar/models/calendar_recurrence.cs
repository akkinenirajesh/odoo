csharp
public partial class RecurrenceRule
{
    public override string ToString()
    {
        return Name;
    }

    public string GetRecurrenceName()
    {
        switch (RruleType)
        {
            case RruleType.Daily:
                return GetDailyRecurrenceName();
            case RruleType.Weekly:
                return GetWeeklyRecurrenceName();
            case RruleType.Monthly:
                return GetMonthlyRecurrenceName();
            case RruleType.Yearly:
                return GetYearlyRecurrenceName();
            default:
                return string.Empty;
        }
    }

    private string GetDailyRecurrenceName()
    {
        if (EndType == EndType.Count)
        {
            return $"Every {Interval} Days for {Count} events";
        }
        if (EndType == EndType.EndDate)
        {
            return $"Every {Interval} Days until {Until:d}";
        }
        return $"Every {Interval} Days";
    }

    private string GetWeeklyRecurrenceName()
    {
        var weekdays = GetWeekDays();
        var dayStrings = weekdays.Select(w => w.ToString()).ToList();
        var days = string.Join(", ", dayStrings);

        if (EndType == EndType.Count)
        {
            return $"Every {Interval} Weeks on {days} for {Count} events";
        }
        if (EndType == EndType.EndDate)
        {
            return $"Every {Interval} Weeks on {days} until {Until:d}";
        }
        return $"Every {Interval} Weeks on {days}";
    }

    private string GetMonthlyRecurrenceName()
    {
        if (MonthBy == MonthBy.Day)
        {
            var positionLabel = Byday.ToString();
            var weekdayLabel = Weekday.ToString();

            if (EndType == EndType.Count)
            {
                return $"Every {Interval} Months on the {positionLabel} {weekdayLabel} for {Count} events";
            }
            if (EndType == EndType.EndDate)
            {
                return $"Every {Interval} Months on the {positionLabel} {weekdayLabel} until {Until:d}";
            }
            return $"Every {Interval} Months on the {positionLabel} {weekdayLabel}";
        }
        else
        {
            if (EndType == EndType.Count)
            {
                return $"Every {Interval} Months day {Day} for {Count} events";
            }
            if (EndType == EndType.EndDate)
            {
                return $"Every {Interval} Months day {Day} until {Until:d}";
            }
            return $"Every {Interval} Months day {Day}";
        }
    }

    private string GetYearlyRecurrenceName()
    {
        if (EndType == EndType.Count)
        {
            return $"Every {Interval} Years for {Count} events";
        }
        if (EndType == EndType.EndDate)
        {
            return $"Every {Interval} Years until {Until:d}";
        }
        return $"Every {Interval} Years";
    }

    private List<DayOfWeek> GetWeekDays()
    {
        var weekDays = new List<DayOfWeek>();
        if (Mon) weekDays.Add(DayOfWeek.Monday);
        if (Tue) weekDays.Add(DayOfWeek.Tuesday);
        if (Wed) weekDays.Add(DayOfWeek.Wednesday);
        if (Thu) weekDays.Add(DayOfWeek.Thursday);
        if (Fri) weekDays.Add(DayOfWeek.Friday);
        if (Sat) weekDays.Add(DayOfWeek.Saturday);
        if (Sun) weekDays.Add(DayOfWeek.Sunday);
        return weekDays;
    }
}
