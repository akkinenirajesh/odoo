csharp
public partial class ResourceResource {

    public ResourceResource() {
    }

    public virtual void ComputeAvatar128() {
        this.Avatar128 = Env.Get("Res.Users").Browse(this.User).Get("Avatar128");
    }

    public virtual ResourceResource Create(Dictionary<string, object> values) {
        if (values.ContainsKey("Company") && !values.ContainsKey("Calendar")) {
            values["Calendar"] = Env.Get("Res.Company").Browse(values["Company"]).Get("ResourceCalendar").Id;
        }
        if (!values.ContainsKey("Tz")) {
            // retrieve timezone on user or calendar
            var tz = (Env.Get("Res.Users").Browse(values.GetValueOrDefault("User")).Get("Tz")
                ?? Env.Get("Resource.Calendar").Browse(values.GetValueOrDefault("Calendar")).Get("Tz"));
            if (tz != null) {
                values["Tz"] = tz;
            }
        }
        return Env.Create(this, values);
    }

    public virtual List<ResourceResource> Create(List<Dictionary<string, object>> valuesList) {
        return Env.Create(this, valuesList);
    }

    public virtual List<Dictionary<string, object>> CopyData(Dictionary<string, object> defaultValues) {
        return Env.CopyData(this, defaultValues);
    }

    public virtual bool Write(Dictionary<string, object> values) {
        if (Env.Context.ContainsKey("CheckIdempotence") && Env.Count(this) == 1) {
            values = values.Where(x => Env.GetField(this, x.Key).ConvertToWrite(Env.Get(this, x.Key), this) != x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        if (!values.Any()) {
            return true;
        }
        return Env.Write(this, values);
    }

    public virtual void OnchangeCompany() {
        if (this.Company != null) {
            this.Calendar = Env.Get("Res.Company").Browse(this.Company).Get("ResourceCalendar").Id;
        }
    }

    public virtual void OnchangeUser() {
        if (this.User != null) {
            this.Tz = Env.Get("Res.Users").Browse(this.User).Get("Tz");
        }
    }

    public virtual Dictionary<string, Tuple<DateTime?, DateTime?>> GetWorkInterval(DateTime start, DateTime end) {
        return AdjustToCalendar(start, end, true);
    }

    public virtual Dictionary<string, Tuple<DateTime?, DateTime?>> AdjustToCalendar(DateTime start, DateTime end, bool computeLeaves) {
        var result = new Dictionary<string, Tuple<DateTime?, DateTime?>>();
        foreach (var resource in Env.Get(this)) {
            var resourceTz = TimeZoneInfo.FindSystemTimeZoneById(resource.Tz);
            var startDateTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.Local, resourceTz);
            var endDateTime = TimeZoneInfo.ConvertTime(end, TimeZoneInfo.Local, resourceTz);

            var searchRange = new List<DateTime> {
                startDateTime.Date,
                endDateTime.AddDays(1).Date
            };

            var calendar = resource.Calendar ?? resource.Company?.ResourceCalendar ?? Env.Company.ResourceCalendar;
            var calendarStart = calendar.GetClosestWorkTime(startDateTime, resource, searchRange, computeLeaves);
            searchRange[0] = startDateTime;
            var calendarEnd = calendar.GetClosestWorkTime(DateTime.Compare(startDateTime, endDateTime) > 0 ? startDateTime : endDateTime, true, resource, searchRange, computeLeaves);

            result.Add(resource.Id, Tuple.Create(calendarStart, calendarEnd));
        }
        return result;
    }

    public virtual Dictionary<string, List<Tuple<DateTime, DateTime>>> GetUnavailableIntervals(DateTime start, DateTime end) {
        var startDateTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.Local, TimeZoneInfo.Utc);
        var endDateTime = TimeZoneInfo.ConvertTime(end, TimeZoneInfo.Local, TimeZoneInfo.Utc);
        var resourceMapping = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();
        var calendarMapping = new Dictionary<int, List<ResourceResource>>();
        foreach (var resource in Env.Get(this)) {
            if (!calendarMapping.ContainsKey(resource.Calendar?.Id ?? resource.Company?.ResourceCalendar?.Id)) {
                calendarMapping[resource.Calendar?.Id ?? resource.Company?.ResourceCalendar?.Id] = new List<ResourceResource>();
            }
            calendarMapping[resource.Calendar?.Id ?? resource.Company?.ResourceCalendar?.Id].Add(resource);
        }
        foreach (var calendar in calendarMapping) {
            var resourcesUnavailableIntervals = Env.Get("Resource.Calendar").Browse(calendar.Key).GetUnavailableIntervalsBatch(startDateTime, endDateTime, calendar.Value, TimeZoneInfo.FindSystemTimeZoneById(calendar.Key == 0 ? Env.Company.ResourceCalendar.Tz : Env.Get("Resource.Calendar").Browse(calendar.Key).Tz));
            foreach (var item in resourcesUnavailableIntervals) {
                resourceMapping[item.Key] = item.Value;
            }
        }
        return resourceMapping;
    }

    public virtual Dictionary<string, Dictionary<int, List<Tuple<DateTime, DateTime>>>> GetCalendarsValidityWithinPeriod(DateTime start, DateTime end, int? defaultCompany = null) {
        var resourceCalendarsWithinPeriod = new Dictionary<string, Dictionary<int, List<Tuple<DateTime, DateTime>>>>();
        var defaultCalendar = defaultCompany != null ? Env.Get("Res.Company").Browse(defaultCompany).ResourceCalendar : Env.Company.ResourceCalendar;
        if (!Env.Count(this).Equals(0)) {
            // if no resource, add the company resource calendar.
            if (!resourceCalendarsWithinPeriod.ContainsKey("false")) {
                resourceCalendarsWithinPeriod.Add("false", new Dictionary<int, List<Tuple<DateTime, DateTime>>>());
            }
            if (!resourceCalendarsWithinPeriod["false"].ContainsKey(defaultCalendar.Id)) {
                resourceCalendarsWithinPeriod["false"].Add(defaultCalendar.Id, new List<Tuple<DateTime, DateTime>> { Tuple.Create(start, end) });
            }
        }
        foreach (var resource in Env.Get(this)) {
            var calendar = resource.Calendar ?? resource.Company?.ResourceCalendar ?? defaultCalendar;
            if (!resourceCalendarsWithinPeriod.ContainsKey(resource.Id.ToString())) {
                resourceCalendarsWithinPeriod.Add(resource.Id.ToString(), new Dictionary<int, List<Tuple<DateTime, DateTime>>>());
            }
            if (!resourceCalendarsWithinPeriod[resource.Id.ToString()].ContainsKey(calendar.Id)) {
                resourceCalendarsWithinPeriod[resource.Id.ToString()].Add(calendar.Id, new List<Tuple<DateTime, DateTime>> { Tuple.Create(start, end) });
            }
        }
        return resourceCalendarsWithinPeriod;
    }

    public virtual Tuple<Dictionary<string, List<Tuple<DateTime, DateTime>>>, Dictionary<int, List<Tuple<DateTime, DateTime>>>> GetValidWorkIntervals(DateTime start, DateTime end, List<int> calendars = null, bool computeLeaves = true) {
        var resourceCalendarValidityIntervals = new Dictionary<string, Dictionary<int, List<Tuple<DateTime, DateTime>>>>();
        var calendarResources = new Dictionary<int, List<ResourceResource>>();
        var resourceWorkIntervals = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();
        var calendarWorkIntervals = new Dictionary<int, List<Tuple<DateTime, DateTime>>>();
        resourceCalendarValidityIntervals = GetCalendarsValidityWithinPeriod(start, end);
        foreach (var resource in Env.Get(this)) {
            foreach (var calendar in resourceCalendarValidityIntervals[resource.Id.ToString()]) {
                if (!calendarResources.ContainsKey(calendar.Key)) {
                    calendarResources.Add(calendar.Key, new List<ResourceResource>());
                }
                calendarResources[calendar.Key].Add(resource);
            }
        }
        if (calendars != null) {
            foreach (var calendar in calendars) {
                if (!calendarResources.ContainsKey(calendar)) {
                    calendarResources.Add(calendar, new List<ResourceResource>());
                }
                calendarResources[calendar].AddRange(Env.Get(this));
            }
        }
        foreach (var calendar in calendarResources) {
            var workIntervalsBatch = Env.Get("Resource.Calendar").Browse(calendar.Key).GetWorkIntervalsBatch(start, end, calendar.Value, computeLeaves);
            foreach (var resource in calendar.Value) {
                resourceWorkIntervals[resource.Id.ToString()] = workIntervalsBatch[resource.Id.ToString()].Intersect(resourceCalendarValidityIntervals[resource.Id.ToString()][calendar.Key]).ToList();
            }
            calendarWorkIntervals[calendar.Key] = workIntervalsBatch["false"];
        }
        return Tuple.Create(resourceWorkIntervals, calendarWorkIntervals);
    }

}
