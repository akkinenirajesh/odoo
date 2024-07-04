csharp
public partial class WebsiteProductPublicCategory 
{
    public int DefaultSequence()
    {
        var category = Env.Search<WebsiteProductPublicCategory>(new object[] { }, 1, "Sequence DESC").FirstOrDefault();
        return category != null ? category.Sequence + 5 : 10000;
    }

    public void ComputeParentsAndSelf()
    {
        if (!string.IsNullOrEmpty(this.ParentPath))
        {
            var parents = this.ParentPath.Split('/').Where(p => !string.IsNullOrEmpty(p)).Select(p => int.Parse(p)).ToArray();
            this.ParentsAndSelf = Env.Browse<WebsiteProductPublicCategory>(parents);
        }
        else
        {
            this.ParentsAndSelf = this;
        }
    }

    public void ComputeDisplayName()
    {
        this.DisplayName = string.Join(" / ", this.ParentsAndSelf.Select(cat => cat.Name ?? "New"));
    }

    public void CheckParentId()
    {
        if (this.HasCycle())
        {
            throw new Exception("Error! You cannot create recursive categories.");
        }
    }

    public object SearchGetDetail(Website website, string order, Dictionary<string, object> options)
    {
        bool withDescription = (bool)options["displayDescription"];
        string[] searchFields = new string[] { "Name" };
        string[] fetchFields = new string[] { "Id", "Name" };
        Dictionary<string, object> mapping = new Dictionary<string, object>()
        {
            { "name", new { name = "Name", type = "text", match = true } },
            { "website_url", new { name = "Url", type = "text", truncate = false } },
        };
        if (withDescription)
        {
            searchFields = searchFields.Append("WebsiteDescription").ToArray();
            fetchFields = fetchFields.Append("WebsiteDescription").ToArray();
            mapping.Add("description", new { name = "WebsiteDescription", type = "text", match = true, html = true });
        }

        return new
        {
            model = "Website.ProductPublicCategory",
            baseDomain = new object[] { website.WebsiteDomain() },
            searchFields = searchFields,
            fetchFields = fetchFields,
            mapping = mapping,
            icon = "fa-folder-o",
            order = "Name DESC, Id DESC".Contains(order) ? "Name DESC, Id DESC" : "Name ASC, Id DESC"
        };
    }

    public List<object> SearchRenderResults(string[] fetchFields, Dictionary<string, object> mapping, string icon, int limit)
    {
        var resultsData = base.SearchRenderResults(fetchFields, mapping, icon, limit);
        foreach (var data in resultsData)
        {
            data["url"] = "/shop/category/" + data["id"];
        }
        return resultsData;
    }

    // Method for cycle detection in parent hierarchy 
    private bool HasCycle()
    {
        // This method needs to be implemented based on the specific structure of the Parent/Child relationship in your ERP system
        // Replace the placeholder logic with appropriate checks for cyclical references
        return false; 
    }
}
