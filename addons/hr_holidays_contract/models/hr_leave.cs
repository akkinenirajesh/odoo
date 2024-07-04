csharp
public partial class HrLeave
{
    public void ComputeResourceCalendarId()
    {
        base.ComputeResourceCalendarId();
        if (this.EmployeeId != null)
        {
            var contracts = Env.Find<HrHolidays.HrContract>(c =>
                (c.State == "open" || c.State == "close" || (c.State == "draft" && c.KanbanState == "done")) &&
                c.EmployeeId == this.EmployeeId &&
                c.DateStart <= this.RequestDateTo &&
                (c.DateEnd == null || c.DateEnd >= this.RequestDateFrom)
            );

            if (contracts.Any())
            {
                this.ResourceCalendarId = contracts.First().ResourceCalendarId;
            }
        }
    }

    public List<HrHolidays.HrContract> GetOverlappingContracts(List<string> contractStates = null)
    {
        if (contractStates == null)
        {
            contractStates = new List<string> { "open", "close" };
        }

        return Env.Find<HrHolidays.HrContract>(c =>
            contractStates.Contains(c.State) &&
            c.EmployeeId == this.EmployeeId &&
            c.DateStart <= this.DateTo &&
            (c.DateEnd >= this.DateFrom || c.DateEnd == null)
        ).ToList();
    }

    public void CheckContracts()
    {
        if (this.EmployeeId != null)
        {
            var contracts = GetOverlappingContracts();
            var uniqueCalendars = contracts.Select(c => c.ResourceCalendarId).Distinct().ToList();

            if (uniqueCalendars.Count > 1)
            {
                var contractDetails = string.Join("\n", contracts.Select(c =>
                    $"Contract {c.Name} from {c.DateStart:d} to {(c.DateEnd.HasValue ? c.DateEnd.Value.ToString("d") : "undefined")}, status: {c.State}"
                ));

                throw new ValidationException(
                    $@"A leave cannot be set across multiple contracts with different working schedules.

Please create one time off for each contract.

Time off:
{this.ToString()}

Contracts:
{contractDetails}"
                );
            }
        }
    }
}
