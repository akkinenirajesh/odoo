C#
public partial class Website {
    public List<Website.Menu> GetSuggestedControllers() {
        List<Website.Menu> suggestedControllers = Env.Get<Website>().GetSuggestedControllers();
        suggestedControllers.Add(new Website.Menu {
            Name = Env.Translate("Blog"),
            Url = Env.GetUrl("/blog"),
            Website = this,
        });
        return suggestedControllers;
    }

    public void ConfiguratorSetMenuLinks(Website.Menu menuCompany, Dictionary<string, object> moduleData) {
        List<object> blogs = moduleData.GetValueOrDefault<List<object>>("#blog", new List<object>());
        foreach (var blog in blogs) {
            var newBlog = Env.Get<Blog.Blog>().Create(new Dictionary<string, object> {
                { "Name", blog.GetValueOrDefault<string>("name") },
                { "Website", this },
            });
            var blogMenuValues = new Dictionary<string, object> {
                { "Name", blog.GetValueOrDefault<string>("name") },
                { "Url", $"/blog/{newBlog.Id}" },
                { "Sequence", blog.GetValueOrDefault<int>("sequence") },
                { "Parent", menuCompany != null ? menuCompany : this.Menu.FirstOrDefault()?.Id },
                { "Website", this },
            };
            var blogMenu = Env.Get<Website.Menu>().Search(new List<object> {
                new List<object> { "Url", "/blog" },
                new List<object> { "Website", this },
            });
            if (blogs.IndexOf(blog) == 0) {
                blogMenu.Update(blogMenuValues);
            } else {
                Env.Get<Website.Menu>().Create(blogMenuValues);
            }
        }
        Env.Get<Website>().ConfiguratorSetMenuLinks(menuCompany, moduleData);
    }

    public List<object> SearchGetDetails(string searchType, string order, Dictionary<string, object> options) {
        var result = Env.Get<Website>().SearchGetDetails(searchType, order, options);
        if (searchType == "blogs" || searchType == "blogs_only" || searchType == "all") {
            result.Add(Env.Get<Blog.Blog>().SearchGetDetail(this, order, options));
        }
        if (searchType == "blogs" || searchType == "blog_posts_only" || searchType == "all") {
            result.Add(Env.Get<Blog.Post>().SearchGetDetail(this, order, options));
        }
        return result;
    }
}
