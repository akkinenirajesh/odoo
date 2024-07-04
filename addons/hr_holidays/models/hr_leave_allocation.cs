csharp
public partial class LeaveAllocation
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionValidate()
    {
        if (State != LeaveAllocationState.Confirm && State != LeaveAllocationState.Validate1 && ValidationType != "no_validation")
        {
            throw new UserException("Allocation must be \"To Approve\" or \"Second Approval\" in order to validate it.");
        }

        if (State == LeaveAllocationState.Confirm && ValidationType != "both")
        {
            State = LeaveAllocationState.Validate;
            ApproverId = Env.User.EmployeeId;
        }
        else if (State == LeaveAllocationState.Validate1 && ValidationType == "both")
        {
            State = LeaveAllocationState.Validate;
            SecondApproverId = Env.User.EmployeeId;
        }

        ActivityUpdate();
    }

    public void ActionSetToConfirm()
    {
        if (State != LeaveAllocationState.Refuse)
        {
            throw new UserException("Allocation state must be \"Refused\" in order to be reset to \"To Approve\".");
        }

        State = LeaveAllocationState.Confirm;
        ApproverId = null;
        SecondApproverId = null;

        ActivityUpdate();
    }

    public void ActionApprove()
    {
        if (State != LeaveAllocationState.Confirm)
        {
            throw new UserException("Allocation must be confirmed (\"To Approve\") in order to approve it.");
        }

        var currentEmployee = Env.User.EmployeeId;

        if (ValidationType == "both")
        {
            State = LeaveAllocationState.Validate1;
            ApproverId = currentEmployee;
        }
        else
        {
            ActionValidate();
        }

        ActivityUpdate();
    }

    public void ActionRefuse()
    {
        if (State != LeaveAllocationState.Confirm && State != LeaveAllocationState.Validate && State != LeaveAllocationState.Validate1)
        {
            throw new UserException("Allocation request must be confirmed, second approval or validated in order to refuse it.");
        }

        var currentEmployee = Env.User.EmployeeId;
        var daysTaken = EmployeeId.GetConsumedLeaves(HolidayStatusId)[this].VirtualLeavesTaken;

        if (daysTaken > 0)
        {
            throw new UserException("You cannot refuse this allocation request since the employee has already taken leaves for it. Please refuse or delete those leaves first.");
        }

        State = LeaveAllocationState.Refuse;
        ApproverId = currentEmployee;

        ActivityUpdate();
    }

    // Other methods would be implemented similarly
}
