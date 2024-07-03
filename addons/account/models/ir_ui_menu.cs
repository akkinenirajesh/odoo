csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();

        if (!Env.User.CompanyIds.Any(company => company.CheckAccountAuditTrail))
        {
            res.Add(Env.Ref("Account.AccountAuditTrailMenu").Id);
        }

        return res;
    }
}
