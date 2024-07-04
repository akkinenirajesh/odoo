csharp
public partial class DiscussChannel
{
    public override string ToString()
    {
        return AnonymousName ?? base.ToString();
    }

    public void ComputeDuration()
    {
        var messages = Env.MessageIds.Where(m => m.ResId == this.Id && m.Model == "ImLivechat.DiscussChannel").OrderByDescending(m => m.Date).ToList();
        var start = messages.LastOrDefault()?.Date ?? this.CreateDate;
        var end = messages.FirstOrDefault()?.Date ?? DateTime.UtcNow;
        this.Duration = (end - start).TotalHours;
    }

    public void ExecuteCommandHistory()
    {
        Env.Bus.SendOne(this, "im_livechat.history_command", new { id = this.Id });
    }

    public void SendHistoryMessage(int partnerId, List<string> pageHistory)
    {
        string messageBody = "No history found";
        if (pageHistory != null && pageHistory.Any())
        {
            var htmlLinks = pageHistory.Select(page => $"<li><a href=\"{System.Web.HttpUtility.HtmlEncode(page)}\" target=\"_blank\">{System.Web.HttpUtility.HtmlEncode(page)}</a></li>");
            messageBody = $"<ul>{string.Join("", htmlLinks)}</ul>";
        }
        SendTransientMessage(Env.Partners.GetById(partnerId), messageBody);
    }

    public string GetVisitorLeaveMessage(bool isOperator = false, bool isCancel = false)
    {
        return "Visitor left the conversation.";
    }

    public void CloseLivechatSession(bool isOperator = false, bool isCancel = false)
    {
        if (this.LivechatActive)
        {
            var member = this.ChannelMembers.FirstOrDefault(m => m.IsSelf);
            if (member != null)
            {
                member.FoldState = "closed";
                member.RtcLeaveCall();
            }
            this.LivechatActive = false;

            if (!this.MessageIds.Any())
            {
                return;
            }

            this.MessagePost(
                authorId: Env.Ref("base.partner_root").Id,
                body: $"<div class=\"o_mail_notification o_hide_author\">{GetVisitorLeaveMessage(isOperator, isCancel)}</div>",
                messageType: "notification",
                subtypeXmlId: "mail.mt_comment"
            );
        }
    }

    // Add other methods as needed
}
