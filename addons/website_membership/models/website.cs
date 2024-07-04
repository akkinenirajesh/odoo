csharp
public partial class Website 
{
    public List<WebsiteSuggestedController> GetSuggestedControllers()
    {
        var suggestedControllers = Env.Call("super", "GetSuggestedControllers") as List<WebsiteSuggestedController>;
        suggestedControllers.Add(new WebsiteSuggestedController { Name = Env.Translate("Members"), Url = "/members", Module = "website_membership" });
        return suggestedControllers;
    }
}
