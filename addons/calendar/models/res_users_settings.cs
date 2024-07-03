csharp
public partial class ResUsersSettings
{
    public override string ToString()
    {
        // You might want to return a meaningful string representation
        return $"User Settings (Calendar Default Privacy: {CalendarDefaultPrivacy})";
    }

    public List<string> GetFieldsBlacklist()
    {
        var blacklist = new List<string> { "CalendarDefaultPrivacy" };
        // Add logic to get the base blacklist if needed
        // blacklist.AddRange(base.GetFieldsBlacklist());
        return blacklist;
    }
}
