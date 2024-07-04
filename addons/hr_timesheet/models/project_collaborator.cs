csharp
public partial class ProjectCollaborator
{
    public void ToggleProjectSharingPortalRules(bool active)
    {
        // Call the base implementation (equivalent to super() in Python)
        base.ToggleProjectSharingPortalRules(active);

        // Access and update ir.model.access
        var accessTimesheetPortal = Env.Ref("hr_timesheet.access_account_analytic_line_portal_user").Sudo();
        if (accessTimesheetPortal.Active != active)
        {
            accessTimesheetPortal.Write(new { Active = active });
        }

        // Access and update ir.rule
        var timesheetPortalIrRule = Env.Ref("hr_timesheet.timesheet_line_rule_portal_user").Sudo();
        if (timesheetPortalIrRule.Active != active)
        {
            timesheetPortalIrRule.Write(new { Active = active });
        }
    }
}
