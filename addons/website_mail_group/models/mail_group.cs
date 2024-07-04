csharp
public partial class Website.MailGroup
{
    public void ActionGoToWebsite()
    {
        var slug = Env.Call("website.ir.http", "Slug", this);
        Env.Call("ir.actions.act_url", "Action", new {
            type = "ir.actions.act_url",
            target = "self",
            url = $"/groups/{slug}"
        });
    }

    public void ComputeWebsiteUrl()
    {
        WebsiteUrl = WebsiteId != null ? WebsiteId.Url : string.Empty;
    }

    public void ComputeMemberCount()
    {
        MemberCount = Followers.Count;
    }
}
