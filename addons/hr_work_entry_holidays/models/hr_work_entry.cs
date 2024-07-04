csharp
public partial class WorkEntry
{
    public bool IsDurationComputedFromCalendar()
    {
        return base.IsDurationComputedFromCalendar() || (WorkEntryTypeId == null && LeaveId != null);
    }

    public override void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("State") && (Hr.WorkEntryState)vals["State"] == Hr.WorkEntryState.Cancelled)
        {
            if (LeaveId != null && LeaveId.State != Hr.LeaveState.Refuse)
            {
                LeaveId.ActionRefuse();
            }
        }
        base.Write(vals);
    }

    public void ResetConflictingState()
    {
        base.ResetConflictingState();
        if (WorkEntryTypeId != null && !WorkEntryTypeId.IsLeave)
        {
            LeaveId = null;
        }
    }

    public bool CheckIfError()
    {
        bool res = base.CheckIfError();
        bool conflictWithLeaves = ComputeConflictsLeavesToApprove();
        return res || conflictWithLeaves;
    }

    public bool ComputeConflictsLeavesToApprove()
    {
        if (this.Id == 0) return false;

        // This method would need to be implemented using the appropriate data access method in C#
        // The SQL query logic would need to be translated to C# and your data access layer

        // For demonstration, let's assume we have a method to execute the query and get results
        var conflicts = ExecuteConflictQuery();

        foreach (var conflict in conflicts)
        {
            Env.Hr.WorkEntry.Browse(conflict.WorkEntryId).Write(new Dictionary<string, object>
            {
                { "State", Hr.WorkEntryState.Conflict },
                { "LeaveId", conflict.LeaveId }
            });
        }

        return conflicts.Any();
    }

    public void ActionApproveLeave()
    {
        if (LeaveId != null)
        {
            if (LeaveId.State == Hr.LeaveState.Validate1)
            {
                LeaveId.ActionValidate();
            }
            else
            {
                LeaveId.ActionApprove();
                if (LeaveId.ValidationType == "both")
                {
                    LeaveId.ActionValidate();
                }
            }
        }
    }

    public void ActionRefuseLeave()
    {
        LeaveId?.ActionRefuse();
    }
}
