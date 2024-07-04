csharp
public partial class WebsiteForumTag {
    public int PostsCount { get; set; }
    public string WebsiteUrl { get; set; }

    public void ComputePostsCount() {
        this.PostsCount = this.Posts.Count;
    }

    public void ComputeWebsiteUrl() {
        this.WebsiteUrl = $"/forum/{Env.Slug(this.ForumId)}/tag/{Env.Slug(this)}/questions";
    }

    public WebsiteForumTag Create(Dictionary<string, object> vals) {
        var forum = Env.Get<WebsiteForum>().Browse(vals["ForumId"]);
        if (Env.User.Karma < forum.KarmaTagCreate && !Env.IsAdmin()) {
            throw new AccessError(string.Format("%d karma required to create a new Tag.", forum.KarmaTagCreate));
        }
        return Env.Get<WebsiteForumTag>().Create(vals);
    }

    public Dictionary<string, object> SearchGetDetail(Website website, string order, Dictionary<string, object> options) {
        var searchFields = new List<string> { "Name" };
        var fetchFields = new List<string> { "Id", "Name", "WebsiteUrl" };
        var mapping = new Dictionary<string, Dictionary<string, object>> {
            {
                "Name", new Dictionary<string, object> {
                    { "name", "Name" },
                    { "type", "text" },
                    { "match", true }
                }
            },
            {
                "WebsiteUrl", new Dictionary<string, object> {
                    { "name", "WebsiteUrl" },
                    { "type", "text" },
                    { "truncate", false }
                }
            }
        };
        var baseDomain = new List<object>();
        if (options.ContainsKey("forum")) {
            var forum = options["forum"];
            var forumIds = forum is string ? new List<int> { Env.Unslug(forum)[1] } : (List<int>)forum;
            var searchDomain = options.ContainsKey("domain") ? (List<object>)options["domain"] : new List<object> { new Dictionary<string, object> { { "ForumId", "in", forumIds } } };
            baseDomain.Add(searchDomain);
        }
        return new Dictionary<string, object> {
            { "model", "Website.ForumTag" },
            { "base_domain", baseDomain },
            { "search_fields", searchFields },
            { "fetch_fields", fetchFields },
            { "mapping", mapping },
            { "icon", "fa-tag" },
            { "order", string.Join(',', order.Split(',').Where(f => !f.Contains("is_published"))) }
        };
    }
}
