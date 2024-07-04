csharp
public partial class Leave
{
    public bool L10nFrLeaveApplies()
    {
        return Employee != null &&
               Company.Country.Code == "FR" &&
               ResourceCalendar != Company.ResourceCalendar &&
               HolidayStatus == Company.GetFrReferenceLeaveType();
    }

    public (DateTime, DateTime) GetFrDateFromTo(DateTime dateFrom, DateTime dateTo)
    {
        // Implement the logic for _get_fr_date_from_to here
        // This method should return a tuple of DateTime objects
        throw new NotImplementedException();
    }

    public void ComputeDateFromTo()
    {
        // Call the base implementation first
        base.ComputeDateFromTo();

        if (L10nFrLeaveApplies())
        {
            var (newDateFrom, newDateTo) = GetFrDateFromTo(DateFrom, DateTo);
            if (newDateFrom != DateFrom)
            {
                DateFrom = newDateFrom;
            }
            if (newDateTo != DateTo)
            {
                DateTo = newDateTo;
                L10nFrDateToChanged = true;
            }
            else
            {
                L10nFrDateToChanged = false;
            }
        }
    }

    public Dictionary<int, double> GetDurations(bool checkLeaveType = true, Resource.Calendar resourceCalendar = null)
    {
        if (resourceCalendar == null)
        {
            var frLeaves = Env.Query<Leave>().Where(leave => leave.L10nFrLeaveApplies()).ToList();
            var durationByLeaveId = base.GetDurations(resourceCalendar: resourceCalendar);
            var frLeavesByCompany = frLeaves.GroupBy(leave => leave.Company);

            foreach (var group in frLeavesByCompany)
            {
                var company = group.Key;
                var leaves = group.ToList();
                var companyDurations = leaves.GetDurations(resourceCalendar: company.ResourceCalendar);
                foreach (var kvp in companyDurations)
                {
                    durationByLeaveId[kvp.Key] = kvp.Value;
                }
            }

            return durationByLeaveId;
        }

        return base.GetDurations(resourceCalendar: resourceCalendar);
    }
}
