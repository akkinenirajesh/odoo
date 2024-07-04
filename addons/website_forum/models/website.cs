csharp
public partial class Website
{
    public int ForumCount { get; set; }

    public Website()
    {
        ForumCount = 0;
    }

    public Website(int forumCount)
    {
        ForumCount = forumCount;
    }

    public void Create(List<Website> websites)
    {
        foreach (Website website in websites)
        {
            website.UpdateForumCount();
        }
    }

    public List<Tuple<string, string, string>> GetSuggestedControllers()
    {
        List<Tuple<string, string, string>> suggestedControllers = Env.Call<List<Tuple<string, string, string>>>("website", "GetSuggestedControllers");
        suggestedControllers.Add(new Tuple<string, string, string>("Forum", "/forum", "website_forum"));
        return suggestedControllers;
    }

    public List<Dictionary<string, string>> ConfiguratorGetFooterLinks()
    {
        List<Dictionary<string, string>> links = Env.Call<List<Dictionary<string, string>>>("website", "ConfiguratorGetFooterLinks");
        links.Add(new Dictionary<string, string>() { { "text", "Forum" }, { "href", "/forum" } });
        return links;
    }

    public void ConfiguratorSetMenuLinks(WebsiteMenu menuCompany, Dictionary<string, object> moduleData)
    {
        WebsiteMenu forumMenu = Env.SearchOne<WebsiteMenu>(x => x.Url == "/forum" && x.WebsiteId == this.Id);
        if (forumMenu != null)
        {
            forumMenu.Unlink();
        }
        Env.Call("website", "ConfiguratorSetMenuLinks", this, menuCompany, moduleData);
    }

    public List<object> SearchGetDetails(string searchType, string order, Dictionary<string, object> options)
    {
        List<object> result = Env.Call<List<object>>("website", "SearchGetDetails", this, searchType, order, options);
        if (searchType == "forums" || searchType == "forums_only" || searchType == "all")
        {
            result.Add(Env.Call<object>("forum.forum", "SearchGetDetail", this, order, options));
        }
        if (searchType == "forums" || searchType == "forum_posts_only" || searchType == "all")
        {
            result.Add(Env.Call<object>("forum.post", "SearchGetDetail", this, order, options));
        }
        if (searchType == "forums" || searchType == "forum_tags_only" || searchType == "all")
        {
            result.Add(Env.Call<object>("forum.tag", "SearchGetDetail", this, order, options));
        }
        return result;
    }

    private void UpdateForumCount()
    {
        List<ForumForum> forumsAll = Env.Search<ForumForum>();
        ForumCount = forumsAll.Where(forum => forum.WebsiteDomain().Contains(this.WebsiteDomain())).Count();
    }

    public string WebsiteDomain()
    {
        // Implement logic to get Website Domain
        return "";
    }
}
