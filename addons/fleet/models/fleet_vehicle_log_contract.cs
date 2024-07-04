csharp
public partial class VehicleLogContract
{
    public string ComputeNextYearDate(DateTime date)
    {
        return date.AddYears(1).ToString("yyyy-MM-dd");
    }

    private void _ComputeContractName()
    {
        string name = Vehicle?.Name;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(CostSubtype?.Name))
        {
            name = $"{CostSubtype.Name} {name}";
        }
        Name = name;
    }

    private void _ComputeDaysLeft()
    {
        var today = DateTime.Today;
        if (ExpirationDate.HasValue && (State == VehicleLogContractState.Open || State == VehicleLogContractState.Expired))
        {
            var diffDays = (ExpirationDate.Value - today).Days;
            DaysLeft = diffDays > 0 ? diffDays : 0;
            ExpiresYoday = diffDays == 0;
        }
        else
        {
            DaysLeft = -1;
            ExpiresYoday = false;
        }
    }

    public void ActionClose()
    {
        State = VehicleLogContractState.Closed;
    }

    public void ActionDraft()
    {
        State = VehicleLogContractState.Futur;
    }

    public void ActionOpen()
    {
        State = VehicleLogContractState.Open;
    }

    public void ActionExpire()
    {
        State = VehicleLogContractState.Expired;
    }

    public void SchedulerManageContractExpiration()
    {
        // Implementation of the scheduler logic
        // This would involve querying the database, updating records, and potentially creating activities
        // The exact implementation would depend on how your C# environment is set up to handle these operations
    }

    public void RunScheduler()
    {
        SchedulerManageContractExpiration();
    }
}
