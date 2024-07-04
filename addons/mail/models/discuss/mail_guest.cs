csharp
public partial class MailGuest
{
    public string Name { get; set; }
    public string AccessToken { get; set; }
    public Core.Country Country { get; set; }
    public string Lang { get; set; }
    public string Timezone { get; set; }
    public IEnumerable<Discuss.Channel> ChannelIds { get; set; }
    public string ImStatus { get; set; }

    public static IEnumerable<KeyValuePair<string, string>> LangGet()
    {
        return Env.Model("res.lang").GetInstalled();
    }

    public void ComputeImStatus()
    {
        // Implementation of ComputeImStatus method
        // Using Env to access other models, services, etc.
        // Example:
        // ImStatus = Env.Model("bus.presence").GetImStatus(this);
    }

    // Other methods and logic for MailGuest model
}
