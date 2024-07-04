csharp
public partial class Leave
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionCancel()
    {
        this.EnsureOne();

        return new {
            name = Env.T("Cancel Time Off"),
            type = "ir.actions.act_window",
            target = "new",
            res_model = "hr.holidays.cancel.leave",
            view_mode = "form",
            views = new[] { new object[] { false, "form" } },
            context = new {
                default_leave_id = this.Id,
            }
        };
    }

    public bool ActionDraft()
    {
        if (this.Any(holiday => holiday.State != LeaveState.Confirm && holiday.State != LeaveState.Refuse))
        {
            throw new UserError(Env.T("Time off request state must be \"Refused\" or \"To Approve\" in order to be reset to draft."));
        }
        this.Write(new {
            State = LeaveState.Draft,
            FirstApproverId = false,
            SecondApproverId = false,
        });
        this.ActivityUpdate();
        return true;
    }

    public bool ActionConfirm()
    {
        var toConfirm = this.Filtered(holiday => holiday.State == LeaveState.Draft);
        toConfirm.Write(new { State = LeaveState.Confirm });
        var holidays = toConfirm.Filtered(leave => leave.ValidationType == "no_validation");
        if (holidays.Any())
        {
            holidays.Sudo().ActionValidate();
        }
        toConfirm.ActivityUpdate();
        return true;
    }

    // ... other action methods ...
}
