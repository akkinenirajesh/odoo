csharp
public partial class Partner
{
    public List<int> GetBackendRootMenuIds()
    {
        var baseMenuIds = base.GetBackendRootMenuIds();
        var contactsMenuId = Env.Ref("contacts.menu_contacts").Id;
        return baseMenuIds.Concat(new List<int> { contactsMenuId }).ToList();
    }
}
