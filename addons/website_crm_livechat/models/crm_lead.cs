csharp
public partial class WebsiteCrmLead 
{
    public int VisitorSessionsCount { get; set; }

    public void ComputeVisitorSessionsCount()
    {
        this.VisitorSessionsCount = Env.GetCollection("Website.CrmLead").Where(l => l.Id == this.Id).FirstOrDefault().VisitorIds.SelectMany(v => v.DiscussChannelIds).Count();
    }

    public dynamic ActionRedirectToLivechatSessions()
    {
        var visitors = Env.GetCollection("Website.CrmLead").Where(l => l.Id == this.Id).FirstOrDefault().VisitorIds;
        dynamic action = Env.GetAction("website_livechat.website_visitor_livechat_session_action");
        action.Domain = new List<dynamic> { new { LivechatVisitorId = visitors.Select(v => v.Id).ToList(), HasMessage = true } };
        return action;
    }
}
