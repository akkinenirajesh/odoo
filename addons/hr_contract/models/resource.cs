csharp
public partial class ResourceCalendar
{
    public void TransferLeavesTo(ResourceCalendar otherCalendar, IEnumerable<Resource> resources = null, DateTime? fromDate = null)
    {
        fromDate = fromDate ?? DateTime.Now.Date;
        var domain = new List<object[]>
        {
            new object[] { "CalendarId", "in", new[] { Id } },
            new object[] { "DateFrom", ">=", fromDate }
        };

        if (resources != null)
        {
            domain.Add(new object[] { "ResourceId", "in", resources.Select(r => r.Id).ToArray() });
        }

        var leavesToTransfer = Env.Set<ResourceCalendarLeave>().Search(domain);
        foreach (var leave in leavesToTransfer)
        {
            leave.CalendarId = otherCalendar.Id;
        }
    }

    public void ComputeContractsCount()
    {
        var contractsCount = Env.Set<HrContract.Contract>()
            .ReadGroup(
                domain: new[] { ("ResourceCalendarId", "=", Id) },
                fields: new[] { "ResourceCalendarId" },
                groupBy: new[] { "ResourceCalendarId" }
            )
            .FirstOrDefault();

        ContractsCount = contractsCount?.Count ?? 0;
    }

    public IrActionsActWindow ActionOpenContracts()
    {
        var action = Env.Ref<IrActionsActWindow>("hr_contract.action_hr_contract");
        action.Domain = new[] { ("ResourceCalendarId", "=", Id) };
        return action;
    }
}
