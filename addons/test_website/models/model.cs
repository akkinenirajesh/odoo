csharp
public partial class TestModel 
{
    public virtual object _SearchGetDetail(Website website, string order, object options)
    {
        return new {
            Model = "test.model",
            BaseDomain = new object[] { },
            SearchFields = new string[] { "Name", "Submodels.Name", "Submodels.TagId.Name" },
            FetchFields = new string[] { "Name" },
            Mapping = new {
                Name = new { Name = "Name", Type = "text", Match = true },
                WebsiteUrl = new { Name = "Name", Type = "text", Truncate = false },
            },
            Icon = "fa-check-square-o",
            Order = "name asc, id desc",
        };
    }
}
