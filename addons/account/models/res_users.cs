csharp
public partial class ResGroups
{
    public List<ResGroups> GetApplicationGroups(List<(string, string, object)> domain)
    {
        var groupAccountUser = Env.Ref("Account.GroupAccountUser", false);
        if (groupAccountUser != null && groupAccountUser.CategoryId.XmlId == "Base.ModuleCategoryHidden")
        {
            domain.Add(("Id", "!=", groupAccountUser.Id));
        }

        var groupAccountReadonly = Env.Ref("Account.GroupAccountReadonly", false);
        if (groupAccountReadonly != null && groupAccountReadonly.CategoryId.XmlId == "Base.ModuleCategoryHidden")
        {
            domain.Add(("Id", "!=", groupAccountReadonly.Id));
        }

        var groupAccountBasic = Env.Ref("Account.GroupAccountBasic", false);
        if (groupAccountBasic != null && groupAccountBasic.CategoryId.XmlId == "Base.ModuleCategoryHidden")
        {
            domain.Add(("Id", "!=", groupAccountBasic.Id));
        }

        return base.GetApplicationGroups(domain);
    }
}
