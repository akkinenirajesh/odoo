C#
public partial class Website
{
    public List<Tuple<string, string, string>> GetSuggestedControllers()
    {
        var suggestedControllers = Env.Call("Website", "get_suggested_controllers").As<List<Tuple<string, string, string>>>();
        suggestedControllers.Add(new Tuple<string, string, string>("Events", "/event", "website_event"));
        return suggestedControllers;
    }

    public Dictionary<string, object> GetCtaData(string websitePurpose, string websiteType)
    {
        var ctaData = Env.Call("Website", "get_cta_data", websitePurpose, websiteType).As<Dictionary<string, object>>();
        if (websitePurpose == "sell_more" && websiteType == "event")
        {
            ctaData["cta_btn_text"] = Env.Translate("Next Events");
            ctaData["cta_btn_href"] = "/event";
        }
        return ctaData;
    }

    public List<Dictionary<string, object>> SearchGetDetails(string searchType, string order, Dictionary<string, object> options)
    {
        var result = Env.Call("Website", "_search_get_details", searchType, order, options).As<List<Dictionary<string, object>>>();
        if (searchType == "events" || searchType == "all")
        {
            result.Add(Env.Call("Event.Event", "_search_get_detail", this, order, options).As<Dictionary<string, object>>());
        }
        return result;
    }
}
