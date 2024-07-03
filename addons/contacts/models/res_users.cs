csharp
public partial class ResUsers
{
    public override string ToString()
    {
        return Name;
    }

    public List<Dictionary<string, object>> GetActivityGroups()
    {
        var activities = base.GetActivityGroups();
        foreach (var activity in activities)
        {
            if ((string)activity["Model"] != "Core.ResPartner")
                continue;

            activity["Icon"] = Env.GetModuleIcon("contacts");
        }
        return activities;
    }
}
