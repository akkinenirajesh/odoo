csharp
using System;
using System.Linq;

public partial class LeaveAccrualLevel
{
    public DateTime GetNextDate(DateTime lastCall)
    {
        switch (Frequency)
        {
            case AccrualFrequency.Hourly:
            case AccrualFrequency.Daily:
                return lastCall.AddDays(1);
            case AccrualFrequency.Weekly:
                return lastCall.AddDays(1).Next((DayOfWeek)Enum.Parse(typeof(DayOfWeek), WeekDay.ToString()));
            case AccrualFrequency.Bimonthly:
                var firstDate = new DateTime(lastCall.Year, lastCall.Month, FirstDay);
                var secondDate = new DateTime(lastCall.Year, lastCall.Month, SecondDay);
                if (lastCall < firstDate) return firstDate;
                if (lastCall < secondDate) return secondDate;
                return new DateTime(lastCall.Year, lastCall.Month, FirstDay).AddMonths(1);
            case AccrualFrequency.Monthly:
                var date = new DateTime(lastCall.Year, lastCall.Month, FirstDay);
                return lastCall < date ? date : date.AddMonths(1);
            case AccrualFrequency.Biyearly:
                var firstMonthDate = new DateTime(lastCall.Year, (int)FirstMonth, FirstMonthDay);
                var secondMonthDate = new DateTime(lastCall.Year, (int)SecondMonth, SecondMonthDay);
                if (lastCall < firstMonthDate) return firstMonthDate;
                if (lastCall < secondMonthDate) return secondMonthDate;
                return new DateTime(lastCall.Year + 1, (int)FirstMonth, FirstMonthDay);
            case AccrualFrequency.Yearly:
                var yearlyDate = new DateTime(lastCall.Year, (int)YearlyMonth, YearlyDay);
                return lastCall < yearlyDate ? yearlyDate : yearlyDate.AddYears(1);
            default:
                throw new ArgumentException("Invalid frequency");
        }
    }

    public DateTime GetPreviousDate(DateTime lastCall)
    {
        switch (Frequency)
        {
            case AccrualFrequency.Hourly:
            case AccrualFrequency.Daily:
                return lastCall;
            case AccrualFrequency.Weekly:
                return lastCall.AddDays(-6).Previous((DayOfWeek)Enum.Parse(typeof(DayOfWeek), WeekDay.ToString()));
            case AccrualFrequency.Bimonthly:
                var secondDate = new DateTime(lastCall.Year, lastCall.Month, SecondDay);
                var firstDate = new DateTime(lastCall.Year, lastCall.Month, FirstDay);
                if (lastCall >= secondDate) return secondDate;
                if (lastCall >= firstDate) return firstDate;
                return new DateTime(lastCall.Year, lastCall.Month, SecondDay).AddMonths(-1);
            case AccrualFrequency.Monthly:
                var date = new DateTime(lastCall.Year, lastCall.Month, FirstDay);
                return lastCall >= date ? date : date.AddMonths(-1);
            case AccrualFrequency.Biyearly:
                var firstMonthDate = new DateTime(lastCall.Year, (int)FirstMonth, FirstMonthDay);
                var secondMonthDate = new DateTime(lastCall.Year, (int)SecondMonth, SecondMonthDay);
                if (lastCall >= secondMonthDate) return secondMonthDate;
                if (lastCall >= firstMonthDate) return firstMonthDate;
                return new DateTime(lastCall.Year - 1, (int)SecondMonth, SecondMonthDay);
            case AccrualFrequency.Yearly:
                var yearlyDate = new DateTime(lastCall.Year, (int)YearlyMonth, YearlyDay);
                return lastCall >= yearlyDate ? yearlyDate : yearlyDate.AddYears(-1);
            default:
                throw new ArgumentException("Invalid frequency");
        }
    }
}
