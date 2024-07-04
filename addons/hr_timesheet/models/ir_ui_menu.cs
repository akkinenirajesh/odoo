csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();
        
        if (Env.User.HasGroup("hr_timesheet.group_hr_timesheet_approver"))
        {
            res.Add(Env.Ref("hr_timesheet.timesheet_menu_activity_user").Id);
        }
        
        return res;
    }
}
