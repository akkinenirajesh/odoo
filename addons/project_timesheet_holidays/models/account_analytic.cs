csharp
public partial class AccountAnalyticLine 
{
    public AccountAnalyticLine()
    {
    }

    public AccountAnalyticLine(int id)
    {
        // Constructor to initialize the object with an id
    }

    private void _UnlinkExceptLinkedLeave()
    {
        if (Env.Context.Contains("active_ids") && Env.Context["active_ids"] is int[] activeIds)
        {
            foreach (var activeId in activeIds)
            {
                var line = new AccountAnalyticLine(activeId);
                if (line.HolidayId != null)
                {
                    if (!Env.User.HasGroup("hr_holidays.group_hr_holidays_user") && !line.HolidayId.UserIds.Contains(Env.User))
                    {
                        throw new Exception("You cannot delete timesheets that are linked to time off requests. Please cancel your time off request from the Time Off application instead.");
                    }
                    var action = _GetRedirectAction();
                    throw new Exception("You cannot delete timesheets that are linked to time off requests. Please cancel your time off request from the Time Off application instead.", action);
                }
            }
        }
        else
        {
            throw new Exception("Active Ids are not found in context");
        }
    }

    private dynamic _GetRedirectAction()
    {
        var leaveFormViewId = Env.Ref("hr_holidays.hr_leave_view_form").Id;
        dynamic actionData = new
        {
            Name = "Time Off",
            Type = "ir.actions.act_window",
            ResModel = "hr.leave",
            Views = new[]
            {
                new[] { Env.Ref("hr_holidays.hr_leave_view_tree_my").Id, "list" },
                new[] { leaveFormViewId, "form" }
            },
            Domain = new[] { "id", "in", HolidayId.Ids }
        };

        if (HolidayId.Count == 1)
        {
            actionData.Views = new[] { new[] { leaveFormViewId, "form" } };
            actionData.ResId = HolidayId.Id;
        }

        return actionData;
    }

    private void _CheckCanWrite(dynamic values)
    {
        if (!Env.User.IsAdmin && HolidayId != null)
        {
            throw new Exception("You cannot modify timesheets that are linked to time off requests. Please use the Time Off application to modify your time off requests instead.");
        }
        // Call the super class method here for _check_can_write
    }

    private void _CheckCanCreate()
    {
        if (!Env.User.IsAdmin && TaskId.Any(t => t.IsTimeoffTask))
        {
            throw new Exception("You cannot create timesheets for a task that is linked to a time off type. Please use the Time Off application to request new time off instead.");
        }
        // Call the super class method here for _check_can_create
    }

    private dynamic _GetFavoriteProjectIdDomain(int? employeeId = null)
    {
        dynamic domain = new[] {
            // Call the super class method for _get_favorite_project_id_domain
            // ...
            new[] { "holiday_id", "=", null },
            new[] { "global_leave_id", "=", null }
        };

        return domain;
    }
}
