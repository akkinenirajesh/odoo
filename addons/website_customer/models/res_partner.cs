C#
public partial class ResPartner {
    public int GetBackendMenuId()
    {
        return Env.Ref("Contacts.MenuContacts").Id;
    }
}

public partial class ResPartnerTag {
    public List<string> GetSelectionClass()
    {
        List<string> classname = new List<string>() { "Light", "Primary", "Success", "Warning", "Danger" };
        return classname.Select(x => (x, x.ToTitleCase())).ToList();
    }

    public bool DefaultIsPublished()
    {
        return true;
    }
}
