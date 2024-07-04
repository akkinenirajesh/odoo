csharp
public partial class Leave
{
    public WorkEntry PrepareResourceLeaveVals()
    {
        var vals = base.PrepareResourceLeaveVals();
        vals.WorkEntryTypeId = this.HolidayStatusId.WorkEntryTypeId;
        return vals;
    }

    public void CancelWorkEntryConflict()
    {
        if (this == null)
            return;

        // Create work entries for each leave
        var workEntriesValsList = new List<WorkEntry>();
        foreach (var leave in this.Env.Context.Leaves)
        {
            var contracts = leave.EmployeeId.GetContracts(leave.DateFrom, leave.DateTo, new[] { "open", "close" });
            foreach (var contract in contracts)
            {
                if (leave.DateTo >= contract.DateGeneratedFrom && leave.DateFrom <= contract.DateGeneratedTo)
                {
                    workEntriesValsList.AddRange(contract.GetWorkEntriesValues(leave.DateFrom, leave.DateTo));
                }
            }
        }

        var newLeaveWorkEntries = this.Env.WorkEntries.Create(workEntriesValsList);

        // Rest of the logic for cancelling work entry conflicts
        // ...
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (this == null)
            return true;

        bool skipCheck = !new[] { "EmployeeId", "State", "RequestDateFrom", "RequestDateTo" }.Any(vals.ContainsKey);
        var employeeIds = this.EmployeeId.Ids;
        if (vals.TryGetValue("EmployeeId", out var employeeId) && employeeId != null)
        {
            employeeIds.Add((int)employeeId);
        }

        // Rest of the write logic
        // ...

        return base.Write(vals);
    }

    public override void Create(List<Dictionary<string, object>> valsList)
    {
        // Create logic
        // ...
    }

    public void ActionConfirm()
    {
        // Confirm action logic
        // ...
    }

    public List<Leave> GetLeavesOnPublicHoliday()
    {
        var leaves = base.GetLeavesOnPublicHoliday();
        return leaves.Where(l => !new[] { "LEAVE110", "LEAVE210", "LEAVE280" }.Contains(l.HolidayStatusId.WorkEntryTypeId.Code)).ToList();
    }

    public bool ValidateLeaveRequest()
    {
        base.ValidateLeaveRequest();
        this.CancelWorkEntryConflict();
        return true;
    }

    public void ActionRefuse()
    {
        // Refuse action logic
        // ...
    }

    public void ActionUserCancel(string reason)
    {
        // User cancel action logic
        // ...
    }

    private void RegenWorkEntries()
    {
        // Regenerate work entries logic
        // ...
    }

    public void ComputeCanCancel()
    {
        base.ComputeCanCancel();

        var cancellableLeaves = this.Env.Context.Leaves.Where(l => l.CanCancel);
        var workEntries = this.Env.WorkEntries.Search(new[]
        {
            ("State", "=", "validated"),
            ("LeaveId", "in", cancellableLeaves.Select(l => l.Id).ToList())
        });
        var leaveIds = workEntries.Select(w => w.LeaveId.Id).ToList();

        foreach (var leave in cancellableLeaves)
        {
            leave.CanCancel = !leaveIds.Contains(leave.Id);
        }
    }
}
