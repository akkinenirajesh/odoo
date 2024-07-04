csharp
public partial class WebsiteUsers
{
    public WebsiteUsers()
    {
    }

    public void OpenWebsiteUrl()
    {
        var partnerId = this.Env.GetRecord("Res.Partner", this.PartnerId);
        partnerId.OpenWebsiteUrl();
    }

    public List<Dictionary<string, object>> GetGamificationRedirectionData()
    {
        var res = this.Env.CallMethod("Website.Users", "GetGamificationRedirectionData");
        res.Add(new Dictionary<string, object> { 
            { "label", Env.Translate("_See our Forum") },
            { "url", "/forum" }
        });
        return res;
    }
}
