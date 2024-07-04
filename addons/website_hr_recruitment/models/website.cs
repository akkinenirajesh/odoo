csharp
public partial class Website {
    public object GetSuggestedControllers() {
        var suggestedControllers = Env.Call<object>("Website", "GetSuggestedControllers");
        suggestedControllers.Append(new object[] { 
            Env.Translate("Jobs"),
            Env.Call<string>("Website", "UrlFor", "/jobs"),
            "website_hr_recruitment" 
        });
        return suggestedControllers;
    }

    public object SearchGetDetails(string searchType, string order, object options) {
        var result = Env.Call<object>("Website", "SearchGetDetails", searchType, order, options);
        if (searchType == "jobs" || searchType == "all") {
            result.Append(Env.Call<object>("HrJob", "_SearchGetDetail", this, order, options));
        }
        return result;
    }
}
