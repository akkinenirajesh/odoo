C#
public partial class WebsiteAttachment {
    public WebsiteAttachment() { }

    public void Create(List<Dictionary<string, object>> valsList) {
        var website = Env.Ref("Website.Website").GetCurrentWebsite(false);
        foreach (var vals in valsList) {
            if (website != null && !vals.ContainsKey("WebsiteId") && !Env.Context.ContainsKey("NotForceWebsiteId")) {
                vals["WebsiteId"] = website.Id;
            }
        }
        var result = Env.Ref("Website.Attachment").Create(valsList);
    }

    public List<string> GetServingGroups() {
        var result = Env.Ref("Website.Attachment").GetServingGroups();
        result.Add("website.group_website_designer");
        return result;
    }

    public object GetServeAttachment(string url, List<string> extraDomain = null, string order = null) {
        var website = Env.Ref("Website.Website").GetCurrentWebsite();
        extraDomain = extraDomain ?? new List<string>();
        extraDomain.AddRange(website.WebsiteDomain());
        order = order != null ? "WebsiteId, " + order : "WebsiteId";
        return Env.Ref("Website.Attachment").GetServeAttachment(url, extraDomain, order);
    }
}
