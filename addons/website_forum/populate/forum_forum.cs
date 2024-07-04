C#
public partial class WebsiteForum 
{
    public WebsiteForumTag[] CreateTags(Random random)
    {
        List<WebsiteForumTag> tags = new List<WebsiteForumTag>();
        for (int i = 0; i < random.Next(1, 7); i++)
        {
            tags.Add(new WebsiteForumTag { Name = $"{this.Name}_tag_{i + 1}" });
        }
        return tags.ToArray();
    }
}
