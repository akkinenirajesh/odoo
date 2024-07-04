csharp
public partial class Leave
{
    public bool ComputeOvertimeDeductible()
    {
        return HolidayStatusId.OvertimeDeductible && HolidayStatusId.RequiresAllocation == "no";
    }

    public void CheckOvertimeDeductible()
    {
        if (!OvertimeDeductible)
            return;

        var employee = EmployeeId.Sudo();
        var duration = NumberOfHours;

        if (duration > employee.TotalOvertime)
        {
            if (employee.UserId == Env.User)
                throw new ValidationException("You do not have enough extra hours to request this leave");
            throw new ValidationException("The employee does not have enough extra hours to request this leave.");
        }

        if (Sudo().OvertimeId == null)
        {
            Sudo().OvertimeId = Env["HrAttendance.Overtime"].Sudo().Create(new Dictionary<string, object>
            {
                ["EmployeeId"] = employee.Id,
                ["Date"] = DateFrom,
                ["Adjustment"] = true,
                ["Duration"] = -1 * duration
            });
        }
    }

    public void ActionDraft()
    {
        if (OvertimeDeductible && EmployeeOvertime < Math.Round(NumberOfHours, 2))
        {
            if (EmployeeId.UserId == Env.User.Id)
                throw new ValidationException("You do not have enough extra hours to request this leave");
            throw new ValidationException("The employee does not have enough extra hours to request this leave.");
        }

        // Assuming base.ActionDraft() is called here

        Sudo().OvertimeId?.Unlink();
        var overtime = Env["HrAttendance.Overtime"].Sudo().Create(new Dictionary<string, object>
        {
            ["EmployeeId"] = EmployeeId.Id,
            ["Date"] = DateFrom,
            ["Adjustment"] = true,
            ["Duration"] = -NumberOfHours
        });
        Sudo().OvertimeId = overtime;
    }

    public void ActionRefuse()
    {
        // Assuming base.ActionRefuse() is called here
        Sudo().OvertimeId?.Unlink();
    }

    public void ValidateLeaveRequest()
    {
        // Assuming base.ValidateLeaveRequest() is called here
        UpdateLeavesOvertime();
    }

    public void RemoveResourceLeave()
    {
        // Assuming base.RemoveResourceLeave() is called here
        UpdateLeavesOvertime();
    }

    private void UpdateLeavesOvertime()
    {
        if (EmployeeId != null && EmployeeCompanyId.HrAttendanceOvertime)
        {
            var attendance = Env["Hr.Attendance"].Sudo();
            var dates = new List<DateTime>();
            for (var d = DateFrom; d <= DateTo; d = d.AddDays(1))
            {
                dates.Add(d);
            }
            attendance.UpdateOvertime(new Dictionary<int, List<DateTime>> { [EmployeeId.Id] = dates });
        }
    }

    public override void Unlink()
    {
        Sudo().OvertimeId?.Unlink();
        base.Unlink();
    }

    public void ForceCancel()
    {
        // Assuming base.ForceCancel() is called here
        Sudo().OvertimeId?.Unlink();
    }
}
