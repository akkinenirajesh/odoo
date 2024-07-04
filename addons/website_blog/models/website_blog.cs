csharp
public partial class WebsiteBlog
{
    public int BlogPostCount { get; set; }

    public void ComputeBlogPostCount()
    {
        this.BlogPostCount = Env.GetCollection<WebsiteBlogPost>().Where(p => p.BlogId == this.Id).Count();
    }

    public void Write(Dictionary<string, object> vals)
    {
        // Call base write
        base.Write(vals);

        if (vals.ContainsKey("Active"))
        {
            // Archiving/unarchiving a blog does it on its posts, too
            var postIds = Env.GetCollection<WebsiteBlogPost>().Where(p => p.BlogId == this.Id).Select(p => p.Id);
            foreach (var postId in postIds)
            {
                var blogPost = Env.GetCollection<WebsiteBlogPost>().Get(postId);
                blogPost.Active = (bool)vals["Active"];
            }
        }
    }

    public int MessagePost(int parentMessageId = 0, int subtypeId = 0, Dictionary<string, object> kwargs = null)
    {
        // Temporary workaround to avoid spam. If someone replies on a channel
        // through the 'Presentation Published' email, it should be considered as a
        // note as we don't want all channel followers to be notified of this answer. 
        if (parentMessageId != 0)
        {
            var parentMessage = Env.GetCollection<MailMessage>().Get(parentMessageId);
            if (parentMessage.Subtype.Id == Env.Ref("WebsiteBlog.mt_blog_blog_published").Id)
            {
                subtypeId = Env.Ref("Mail.mt_note").Id;
            }
        }
        return base.MessagePost(parentMessageId, subtypeId, kwargs);
    }

    public List<WebsiteBlogTag> AllTags(bool join = false, int minLimit = 1)
    {
        var sql = @"
            SELECT
                p.BlogId, COUNT(*), r.BlogTagId
            FROM
                BlogPostBlogTagRel r
                    JOIN BlogPost p ON r.BlogPostId = p.Id
            WHERE
                p.BlogId IN (@blogIds)
            GROUP BY
                p.BlogId,
                r.BlogTagId
            ORDER BY
                COUNT(*) DESC
        ";

        var tagByBlog = new Dictionary<int, List<int>>();
        var allTags = new HashSet<int>();

        foreach (var blogId in this.Id)
        {
            tagByBlog.Add(blogId, new List<int>());
        }

        var blogIds = this.Id.ToArray();
        var sqlParams = new Dictionary<string, object> { { "@blogIds", blogIds } };

        var results = Env.ExecuteSql(sql, sqlParams);

        foreach (var row in results)
        {
            var blogId = (int)row[0];
            var frequency = (int)row[1];
            var tagId = (int)row[2];

            if (frequency >= minLimit)
            {
                if (join)
                {
                    allTags.Add(tagId);
                }
                else
                {
                    tagByBlog[blogId].Add(tagId);
                }
            }
        }

        if (join)
        {
            return Env.GetCollection<WebsiteBlogTag>().Where(t => allTags.Contains(t.Id)).ToList();
        }
        else
        {
            foreach (var blogId in tagByBlog.Keys)
            {
                tagByBlog[blogId] = Env.GetCollection<WebsiteBlogTag>().Where(t => tagByBlog[blogId].Contains(t.Id)).ToList();
            }
            return tagByBlog.Values.ToList();
        }
    }

    public Dictionary<string, object> SearchGetDetail(Website website, string order, Dictionary<string, object> options)
    {
        var withDescription = options["displayDescription"] as bool;
        var mapping = new Dictionary<string, Dictionary<string, object>>
        {
            {
                "Name", new Dictionary<string, object>
                {
                    { "Name", "Name" },
                    { "Type", "text" },
                    { "Match", true }
                }
            },
            {
                "WebsiteUrl", new Dictionary<string, object>
                {
                    { "Name", "Url" },
                    { "Type", "text" },
                    { "Truncate", false }
                }
            }
        };

        if (withDescription)
        {
            mapping.Add("Description", new Dictionary<string, object>
            {
                { "Name", "Subtitle" },
                { "Type", "text" },
                { "Match", true }
            });
        }

        return new Dictionary<string, object>
        {
            { "Model", "Website.Blog" },
            { "BaseDomain", new List<object> { website.WebsiteDomain() } },
            { "SearchFields", new List<string> { "Name", "Subtitle" } },
            { "FetchFields", new List<string> { "Id", "Name", "Subtitle" } },
            { "Mapping", mapping },
            { "Icon", "fa-rss-square" },
            { "Order", (order.Contains("Name desc") ? "Name desc, Id desc" : "Name asc, Id desc") }
        };
    }

    public List<Dictionary<string, object>> SearchRenderResults(List<string> fetchFields, Dictionary<string, Dictionary<string, object>> mapping, string icon, int limit)
    {
        var resultsData = base.SearchRenderResults(fetchFields, mapping, icon, limit);
        foreach (var data in resultsData)
        {
            data["Url"] = $"/blog/{data["Id"]}";
        }
        return resultsData;
    }
}
