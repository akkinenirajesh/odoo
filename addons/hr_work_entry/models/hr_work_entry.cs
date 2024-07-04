csharp
public partial class WorkEntry
{
    public override string ToString()
    {
        return Name;
    }

    private void _ComputeName()
    {
        if (Employee == null)
        {
            Name = "Undefined";
        }
        else
        {
            Name = $"{WorkEntryType?.Name ?? "Undefined Type"}: {Employee.Name}";
        }
    }

    private void _ComputeConflict()
    {
        Conflict = State == WorkEntryState.Conflict;
    }

    private void _ComputeDuration()
    {
        if (DateStart.HasValue && DateStop.HasValue)
        {
            Duration = (DateStop.Value - DateStart.Value).TotalHours;
        }
        else
        {
            Duration = 0;
        }
    }

    private void _ComputeDateStop()
    {
        if (DateStart.HasValue && Duration.HasValue)
        {
            DateStop = DateStart.Value.AddHours(Duration.Value);
        }
    }

    public bool ActionValidate()
    {
        if (State != WorkEntryState.Validated && !CheckIfError())
        {
            State = WorkEntryState.Validated;
            return true;
        }
        return false;
    }

    private bool CheckIfError()
    {
        if (WorkEntryType == null)
        {
            State = WorkEntryState.Conflict;
            return true;
        }
        
        // Implement conflict checking logic here
        // You may need to use Env to query other work entries and check for overlaps
        
        return false;
    }
}
