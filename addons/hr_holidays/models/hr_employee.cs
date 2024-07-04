csharp
public partial class HrEmployeeBase
{
    public float GetRemainingLeaves()
    {
        // Implementation for computing remaining leaves
        // This is a placeholder and should be replaced with actual logic
        return 0;
    }

    public void ComputeLeaveStatus()
    {
        // Implementation for computing leave status
        // This is a placeholder and should be replaced with actual logic
    }

    public void ComputeLeaveManager()
    {
        // Implementation for computing leave manager
        // This is a placeholder and should be replaced with actual logic
    }

    public void ComputeShowLeaves()
    {
        // Implementation for computing show leaves
        // This is a placeholder and should be replaced with actual logic
    }

    public void ComputeAllocationRemainingDisplay()
    {
        // Implementation for computing allocation and remaining display
        // This is a placeholder and should be replaced with actual logic
    }

    public void ComputePresenceIcon()
    {
        // Implementation for computing presence icon
        // This is a placeholder and should be replaced with actual logic
    }

    public Dictionary<string, string> GetMandatoryDays(DateTime startDate, DateTime endDate)
    {
        // Implementation for getting mandatory days
        // This is a placeholder and should be replaced with actual logic
        return new Dictionary<string, string>();
    }

    public List<Hr.LeaveAllocation> GetPublicHolidays(DateTime dateStart, DateTime dateEnd)
    {
        // Implementation for getting public holidays
        // This is a placeholder and should be replaced with actual logic
        return new List<Hr.LeaveAllocation>();
    }

    public List<Hr.LeaveMandatoryDay> GetMandatoryDays(DateTime startDate, DateTime endDate)
    {
        // Implementation for getting mandatory days
        // This is a placeholder and should be replaced with actual logic
        return new List<Hr.LeaveMandatoryDay>();
    }

    public (Dictionary<Hr.Employee, Dictionary<Hr.LeaveType, Dictionary<Hr.LeaveAllocation, Dictionary<string, float>>>> allocationsLeavesConsumed,
            Dictionary<Hr.Employee, Dictionary<Hr.LeaveType, Dictionary<string, object>>> toRecheckLeavesPerLeaveType)
    GetConsumedLeaves(List<Hr.LeaveType> leaveTypes, DateTime? targetDate = null, bool ignoreFuture = false)
    {
        // Implementation for getting consumed leaves
        // This is a placeholder and should be replaced with actual logic
        return (new Dictionary<Hr.Employee, Dictionary<Hr.LeaveType, Dictionary<Hr.LeaveAllocation, Dictionary<string, float>>>>(),
                new Dictionary<Hr.Employee, Dictionary<Hr.LeaveType, Dictionary<string, object>>>());
    }
}
