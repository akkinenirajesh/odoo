csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();
        
        if (Env.User.HasGroup("hr.group_hr_user"))
        {
            res.Add(Env.Ref("hr.menu_hr_employee").Id);
        }
        
        return res;
    }
}
