csharp
public partial class WebsiteSlidesForum.ForumForum {
    public WebsiteSlidesForum.ForumForum(Buvi.Env env) {
        Env = env;
    }

    public Buvi.Env Env { get; private set; }

    public IEnumerable<WebsiteSlides.SlideChannel> SlideChannelIds { get; set; }

    public string Name { get; set; }

    public Dictionary<string, int> PopulateSizes() {
        return this.PopulateSizesCore(this.Env);
    }

    private Dictionary<string, int> PopulateSizesCore(Buvi.Env env) {
        var superPopulateSizes = env.Call("WebsiteSlidesForum.ForumForum", "PopulateSizes").Result<Dictionary<string, int>>();
        var slideChannelPopulateSizes = env.Call("WebsiteSlides.SlideChannel", "PopulateSizes").Result<Dictionary<string, int>>();

        var result = new Dictionary<string, int>();
        foreach (var keyValuePair in superPopulateSizes) {
            result.Add(keyValuePair.Key, keyValuePair.Value + slideChannelPopulateSizes[keyValuePair.Key]);
        }

        return result;
    }

    public IEnumerable<string> PopulateDependencies() {
        return this.PopulateDependenciesCore(this.Env);
    }

    private IEnumerable<string> PopulateDependenciesCore(Buvi.Env env) {
        var superPopulateDependencies = env.Call("WebsiteSlidesForum.ForumForum", "PopulateDependencies").Result<IEnumerable<string>>();
        return superPopulateDependencies.Concat(new List<string> { "WebsiteSlides.SlideChannel" });
    }

    public IEnumerable<Tuple<string, Delegate>> PopulateFactories() {
        return this.PopulateFactoriesCore(this.Env);
    }

    private IEnumerable<Tuple<string, Delegate>> PopulateFactoriesCore(Buvi.Env env) {
        var superPopulateFactories = env.Call("WebsiteSlidesForum.ForumForum", "PopulateFactories").Result<IEnumerable<Tuple<string, Delegate>>>();
        var courses = env.Browse("WebsiteSlides.SlideChannel");
        var linkCourse = new Func<IEnumerable<Dictionary<string, object>>, IEnumerable<Dictionary<string, object>>>(iterator => {
            var values = iterator.ToList();
            var courseList = courses.ToList();
            var result = new List<Dictionary<string, object>>();
            for (int i = 0; i < values.Count; i++) {
                var course = i < courseList.Count ? courseList[i] : null;
                var value = values[i];
                if (course != null) {
                    value["SlideChannelIds"] = course;
                    value["Name"] = $"{course.Name}'s Forum";
                }
                result.Add(value);
            }

            return result;
        });

        return superPopulateFactories.Concat(new List<Tuple<string, Delegate>> { new Tuple<string, Delegate>("_NameAndCourse", linkCourse) });
    }
}
