csharp
public partial class Website
{
    public Website(Env env)
    {
        this.Env = env;
    }

    public Env Env { get; }

    public List<Tuple<string, string, string>> GetSuggestedControllers()
    {
        List<Tuple<string, string, string>> suggestedControllers = this.Env.Call("super", "GetSuggestedControllers") as List<Tuple<string, string, string>>;
        suggestedControllers.Add(Tuple.Create("Courses", "/slides", "website_slides"));
        return suggestedControllers;
    }

    public List<object> SearchGetDetails(string searchType, string order, object options)
    {
        List<object> result = this.Env.Call("super", "_search_get_details", searchType, order, options) as List<object>;
        if (searchType == "slides" || searchType == "slide_channels_only" || searchType == "all")
        {
            result.Add(this.Env["slide.channel"].Call("_search_get_detail", this, order, options));
        }
        if (searchType == "slides" || searchType == "slides_only" || searchType == "all")
        {
            result.Add(this.Env["slide.slide"].Call("_search_get_detail", this, order, options));
        }
        return result;
    }
}
