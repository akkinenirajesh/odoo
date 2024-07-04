csharp
public partial class WorkEntry
{
    public override string ToString()
    {
        // Implement string representation logic here
        return $"Work Entry: {EmployeeId?.Name} - {DateStart:d} to {DateStop:d}";
    }

    public void OnChangeEmployeeId()
    {
        OnChangeContractId();
    }

    public void OnChangeDateStart()
    {
        OnChangeContractId();
    }

    public void OnChangeDateStop()
    {
        OnChangeContractId();
    }

    public void OnChangeContractId()
    {
        var vals = new Dictionary<string, object>
        {
            ["EmployeeId"] = EmployeeId?.Id,
            ["DateStart"] = DateStart,
            ["DateStop"] = DateStop
        };

        try
        {
            var res = SetCurrentContract(vals);
            if (res.TryGetValue("ContractId", out var contractId))
            {
                ContractId = Env.Get<Hr.Contract>().Browse((int)contractId);
            }
        }
        catch (ValidationException)
        {
            // Handle validation error
        }
    }

    public bool GetDurationIsValid()
    {
        return WorkEntryTypeId != null && WorkEntryTypeId.IsLeave;
    }

    public void ComputeDateStop()
    {
        if (GetDurationIsValid())
        {
            var calendar = ContractId?.ResourceCalendarId;
            if (calendar == null)
            {
                return;
            }
            DateStop = calendar.PlanHours(Duration, DateStart, computeLeaves: true);
        }
        else
        {
            // Call base implementation
            base.ComputeDateStop();
        }
    }

    public bool IsDurationComputedFromCalendar()
    {
        return GetDurationIsValid();
    }

    private Dictionary<string, object> SetCurrentContract(Dictionary<string, object> vals)
    {
        if (!vals.ContainsKey("ContractId") && vals.ContainsKey("DateStart") && vals.ContainsKey("DateStop") && vals.ContainsKey("EmployeeId"))
        {
            var contractStart = ((DateTime)vals["DateStart"]).Date;
            var contractEnd = ((DateTime)vals["DateStop"]).Date;
            var employee = Env.Get<Hr.Employee>().Browse((int)vals["EmployeeId"]);
            var contracts = employee.GetContracts(contractStart, contractEnd, new[] { "open", "pending", "close" });

            if (contracts.Count == 0)
            {
                throw new ValidationException($"{employee.Name} does not have a contract from {contractStart:d} to {contractEnd:d}.");
            }
            else if (contracts.Count > 1)
            {
                throw new ValidationException($"{employee.Name} has multiple contracts from {contractStart:d} to {contractEnd:d}. A work entry cannot overlap multiple contracts.");
            }

            vals["ContractId"] = contracts[0].Id;
        }
        return vals;
    }

    // Add other methods as needed
}
