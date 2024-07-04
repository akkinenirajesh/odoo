csharp
public partial class CalendarLeaves
{
    public override string ToString()
    {
        // Implement a meaningful string representation
        return $"Calendar Leave: {DateFrom} - {DateTo}";
    }

    public void CheckCompareDates()
    {
        var allExistingLeaves = Env.Set<CalendarLeaves>().Search(new[]
        {
            ("ResourceId", "=", null),
            ("CompanyId", "in", this.CompanyId.Ids),
            ("DateFrom", "<=", this.Set().Max(l => l.DateTo)),
            ("DateTo", ">=", this.Set().Min(l => l.DateFrom))
        });

        foreach (var record in this.Set())
        {
            if (record.ResourceId == null)
            {
                var existingLeaves = allExistingLeaves.Where(leave =>
                    record.Id != leave.Id &&
                    record.CompanyId == leave.CompanyId &&
                    record.DateFrom <= leave.DateTo &&
                    record.DateTo >= leave.DateFrom);

                if (record.CalendarId != null)
                {
                    existingLeaves = existingLeaves.Where(l => l.CalendarId == null || l.CalendarId == record.CalendarId);
                }

                if (existingLeaves.Any())
                {
                    throw new ValidationException("Two public holidays cannot overlap each other for the same working hours.");
                }
            }
        }
    }

    public List<Dictionary<string, object>> GetTimeDomainDict()
    {
        return this.Set()
            .Where(record => record.ResourceId == null)
            .Select(record => new Dictionary<string, object>
            {
                ["company_id"] = record.CompanyId.Id,
                ["date_from"] = record.DateFrom,
                ["date_to"] = record.DateTo
            })
            .ToList();
    }

    public void ReevaluateLeaves(List<Dictionary<string, object>> timeDomainDict)
    {
        if (timeDomainDict.Count == 0)
        {
            return;
        }

        var domain = GetDomain(timeDomainDict);
        var leaves = Env.Set<Hr.Leave>().Search(domain);

        if (leaves.Count == 0)
        {
            return;
        }

        // Implement the rest of the reevaluation logic here
        // This will involve updating leave states, recalculating durations,
        // and potentially notifying users of changes
    }

    // Add other methods like ConvertTimezone, EnsureDatetime, etc.
}
