csharp
public partial class Contract
{
    public List<WorkEntryValues> GetContractWorkEntriesValues(DateTime dateStart, DateTime dateStop)
    {
        var result = base.GetContractWorkEntriesValues(dateStart, dateStop);

        var frContracts = Env.Query<Contract>()
            .Where(c => c.Company.Country.Code == "FR" && c.ResourceCalendar != c.Company.ResourceCalendar)
            .ToList();

        if (!frContracts.Any())
        {
            return result;
        }

        var startDt = dateStart.ToUniversalTime();
        var endDt = dateStop.ToUniversalTime();

        var allLeaves = Env.Query<HumanResources.Leave>()
            .Where(l => frContracts.Select(c => c.Employee.Id).Contains(l.Employee.Id))
            .Where(l => l.State == "validate")
            .Where(l => l.DateFrom <= endDt && l.DateTo >= startDt)
            .Where(l => l.FrDateToChanged)
            .ToList();

        var leavesPerEmployee = allLeaves.GroupBy(l => l.Employee)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var contract in frContracts)
        {
            var employee = contract.Employee;
            var employeeCalendar = contract.ResourceCalendar;
            var company = contract.Company;
            var companyCalendar = company.ResourceCalendar;
            var resource = employee.Resource;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(employeeCalendar.Tz);

            if (leavesPerEmployee.TryGetValue(employee, out var employeeLeaves))
            {
                foreach (var leave in employeeLeaves)
                {
                    var leaveStartDt = Max(startDt, leave.DateFrom.ToTimeZone(tz));
                    var leaveEndDtFr = Min(endDt, leave.DateTo.ToTimeZone(tz));

                    var companyAttendances = companyCalendar.GetAttendanceIntervalsBatch(
                        leaveStartDt, leaveEndDtFr, new[] { resource }, tz)[resource.Id];

                    var employeeDates = result.SelectMany(v => new[] { v.DateStart.Date, v.DateStop.Date }).ToHashSet();

                    var leaveWorkEntryType = leave.HolidayStatus.WorkEntryType;

                    result.AddRange(companyAttendances
                        .Where(interval => !employeeDates.Contains(interval.Start.Date))
                        .Select(interval => new WorkEntryValues
                        {
                            Name = $"{(leaveWorkEntryType != null ? leaveWorkEntryType.Name + ": " : "")}{employee.Name}",
                            DateStart = interval.Start.ToUniversalTime(),
                            DateStop = interval.End.ToUniversalTime(),
                            WorkEntryType = leaveWorkEntryType,
                            Employee = employee,
                            Company = contract.Company,
                            State = "draft",
                            Contract = contract,
                            Leave = leave
                        }));
                }
            }
        }

        return result;
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
