csharp
public partial class MailMessage
{
    public string ComputeParentAuthorName()
    {
        var author = ParentId?.AuthorId ?? ParentId?.AuthorGuestId;
        return author?.Name;
    }

    public string ComputeParentBody()
    {
        return ParentId?.Body;
    }

    public Dictionary<string, object> MessageFormat(bool formatReply = true, Dictionary<string, object> msgVals = null, bool forCurrentUser = false)
    {
        var result = base.MessageFormat(formatReply, msgVals, forCurrentUser);

        var discussChannel = Model == "Discuss.Channel" 
            ? Env.Get<DiscussChannel>().Browse(ResId) 
            : null;

        if (discussChannel?.ChannelType == "livechat")
        {
            if (AuthorId != null)
            {
                result.Remove("EmailFrom");
            }

            if (AuthorId?.UserLivechatUsername != null)
            {
                ((Dictionary<string, object>)result["Author"]).Remove("Name");
                ((Dictionary<string, object>)result["Author"])["UserLivechatUsername"] = AuthorId.UserLivechatUsername;
            }

            if (discussChannel.ChatbotCurrentStepId != null && 
                AuthorId == discussChannel.ChatbotCurrentStepId.ChatbotScriptId.OperatorPartnerId)
            {
                var chatbotMessage = Env.Get<ChatbotMessage>().Search(new[]
                {
                    new[] { "MailMessageId", "=", Id }
                }, limit: 1).FirstOrDefault();

                if (chatbotMessage?.ScriptStepId != null)
                {
                    result["ChatbotStep"] = new Dictionary<string, object>
                    {
                        ["Message"] = new Dictionary<string, object> { ["Id"] = Id },
                        ["ScriptStep"] = new Dictionary<string, object> { ["Id"] = chatbotMessage.ScriptStepId.Id },
                        ["Chatbot"] = new Dictionary<string, object>
                        {
                            ["Script"] = new Dictionary<string, object> { ["Id"] = chatbotMessage.ScriptStepId.ChatbotScriptId.Id },
                            ["Thread"] = new Dictionary<string, object> { ["Id"] = discussChannel.Id, ["Model"] = "Discuss.Channel" }
                        },
                        ["SelectedAnswer"] = chatbotMessage.UserScriptAnswerId != null
                            ? new Dictionary<string, object> { ["Id"] = chatbotMessage.UserScriptAnswerId.Id }
                            : null,
                        ["OperatorFound"] = discussChannel.ChatbotCurrentStepId.StepType == "forward_operator" && discussChannel.ChannelMemberIds.Count() > 2
                    };
                }
            }
        }

        return result;
    }
}
