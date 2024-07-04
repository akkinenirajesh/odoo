csharp
public partial class WebsiteLivechatChannel 
{
    public DiscussChannel GetLivechatDiscussChannelVals(string anonymousName, ResUsers previousOperatorId = null, ChatbotScript chatbotScript = null, ResUsers userId = null, CoreCountry countryId = null, string lang = null)
    {
        DiscussChannel discussChannelVals =  Env.Call<DiscussChannel>("_get_livechat_discuss_channel_vals", anonymousName, previousOperatorId, chatbotScript, userId, countryId, lang);
        if (discussChannelVals == null)
        {
            return null;
        }
        WebsiteVisitor visitorSudo = Env.Call<WebsiteVisitor>("_get_visitor_from_request");
        if (visitorSudo != null)
        {
            discussChannelVals.LivechatVisitorId = visitorSudo;
            // As chat requested by the visitor, delete the chat requested by an operator if any to avoid conflicts between two flows
            // TODO DBE : Move this into the proper method (open or init mail channel)
            DiscussChannel[] chatRequestChannel = Env.Call<DiscussChannel>("search", new object[] { new[] { "LivechatVisitorId", "=", visitorSudo }, new[] { "LivechatActive", "=", true } });
            foreach (DiscussChannel discussChannel in chatRequestChannel)
            {
                ResUsers operator = discussChannel.LivechatOperatorId;
                string operatorName = operator.UserLivechatUsername ?? operator.Name;
                discussChannel.CloseLivechatSession(true, operatorName);
            }
        }

        return discussChannelVals;
    }
}
