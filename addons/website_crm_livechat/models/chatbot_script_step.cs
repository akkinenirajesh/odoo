csharp
public partial class WebsiteChatbotScriptStep
{
    public WebsiteChatbotScriptStep ChatbotCrmPrepareLeadValues(WebsiteDiscussChannel discussChannel, string description)
    {
        WebsiteChatbotScriptStep values = base.ChatbotCrmPrepareLeadValues(discussChannel, description);
        if (discussChannel.LivechatVisitorId != null)
        {
            values.Name = Env.Translate("%s's New Lead", discussChannel.LivechatVisitorId.DisplayName);
            values.LivechatVisitor = discussChannel.LivechatVisitorId;
        }
        return values;
    }
}
