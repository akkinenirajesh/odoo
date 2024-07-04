csharp
public partial class Website
{
    public List<Tuple<string, string, string>> GetSuggestedControllers()
    {
        var suggestedControllers = Env.Call<List<Tuple<string, string, string>>>("website.website", "get_suggested_controllers");
        suggestedControllers.Add(new Tuple<string, string, string>("Resellers", Env.Call<string>("website", "url_for", "/partners"), "website_crm_partner_assign"));
        return suggestedControllers;
    }
}
