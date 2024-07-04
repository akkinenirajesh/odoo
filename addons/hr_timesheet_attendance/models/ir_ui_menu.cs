csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();
        
        if (!Env.User.HasGroup("hr_timesheet.group_hr_timesheet_user"))
        {
            var menuId = Env.Ref("hr_timesheet_attendance.menu_hr_timesheet_attendance_report").Id;
            res.Add(menuId);
        }
        
        return res;
    }
}
