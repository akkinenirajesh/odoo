C#
public partial class ResourceCalendarLeaves {
    public void ComputeCompanyId() {
        this.Company = this.Calendar.Company ?? Env.Company;
    }

    public void ComputeDateTo() {
        if (!this.DateFrom || (this.DateTo && this.DateTo > this.DateFrom)) {
            return;
        }
        var userTz = TimeZoneInfo.FindSystemTimeZoneById(Env.User.TimeZone ?? Env.Context.Get<string>("tz") ?? this.Company.Calendar.TimeZone ?? "UTC");
        var localDateFrom = TimeZoneInfo.ConvertTimeFromUtc(this.DateFrom.ToUniversalTime(), userTz);
        var localDateTo = localDateFrom.AddHours(23).AddMinutes(59).AddSeconds(59);
        this.DateTo = TimeZoneInfo.ConvertTimeToUtc(localDateTo, userTz);
    }

    public void CheckDates() {
        if (this.DateFrom > this.DateTo) {
            throw new Exception("The start date of the time off must be earlier than the end date.");
        }
    }

    public void OnChangeResource() {
        if (this.Resource != null) {
            this.Calendar = this.Resource.Calendar;
        }
    }

    public ResourceCalendarLeaves CopyLeaveVals() {
        return new ResourceCalendarLeaves {
            Name = this.Name,
            DateFrom = this.DateFrom,
            DateTo = this.DateTo,
            TimeType = this.TimeType,
        };
    }
}
