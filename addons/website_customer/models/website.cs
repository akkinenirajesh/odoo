csharp
public partial class Website {
    public object GetSuggestedControllers() {
        var suggestedControllers = Env.Call("Website", "get_suggested_controllers");
        suggestedControllers.Append(new object[] { Env.Translate("References"), Env.Call("url_for", "/customers"), "website_customer" });
        return suggestedControllers;
    }
}
