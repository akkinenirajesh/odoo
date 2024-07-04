csharp
public partial class ResUsersSettings {
    public ResUsersSettings() { }

    public List<string> GetFieldsBlacklist() {
        var microsoftFieldsBlacklist = new List<string>() {
            "MicrosoftCalendarSyncToken",
            "MicrosoftSynchronizationStopped",
            "MicrosoftLastSyncDate"
        };

        var superBlacklist = Env.Ref("ResUsersSettings").GetFieldsBlacklist(); // GetFieldsBlacklist is assumed to be a method of the super class.
        return superBlacklist.Concat(microsoftFieldsBlacklist).ToList();
    }
}
