csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();
        
        var isContractEmployeeManager = Env.User.HasGroup("HR.Contract.GroupHrContractEmployeeManager");
        var isEmployeeOfficer = Env.User.HasGroup("HR.GroupHrUser");
        
        if (!isContractEmployeeManager || isEmployeeOfficer)
        {
            var menuId = Env.Ref("HR.Contract.MenuHrEmployeeContracts").Id;
            res.Add(menuId);
        }
        
        return res;
    }
}
