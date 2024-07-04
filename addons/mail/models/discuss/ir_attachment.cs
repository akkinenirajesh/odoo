csharp
public partial class MailIrAttachment 
{
    public virtual DiscussChannel GetBusNotificationTarget()
    {
        if (this.ResModel == "Discuss.Channel" && this.ResId > 0)
        {
            return Env.Get("Discuss.Channel").Browse(this.ResId);
        }

        MailGuest guest = Env.Get("Mail.Guest").GetGuestFromContext();
        if (Env.User.IsPublic() && guest != null)
        {
            return guest;
        }

        return Env.Get("Ir.Attachment").GetBusNotificationTarget();
    }

    public virtual List<Dictionary<string, object>> GetAttachmentFormat()
    {
        List<Dictionary<string, object>> attachmentFormat = Env.Get("Ir.Attachment").GetAttachmentFormat();
        foreach (Dictionary<string, object> a in attachmentFormat)
        {
            // sudo: discuss.voice.metadata - checking the existence of voice metadata for accessible attachments is fine
            a["Voice"] = this.Browse(a["id"]).WithPrefetch(this.PrefetchIds).Sudo().VoiceIds.Any();
        }

        return attachmentFormat;
    }

    public virtual void PostAddCreate(Dictionary<string, object> kwargs)
    {
        Env.Get("Ir.Attachment").PostAddCreate(kwargs);

        if (kwargs.ContainsKey("Voice") && (bool)kwargs["Voice"])
        {
            List<object> createValues = new List<object>();
            foreach (MailIrAttachment attachment in this)
            {
                createValues.Add(new Dictionary<string, object>() { {"AttachmentId", attachment.Id} });
            }
            Env.Get("Discuss.VoiceMetadata").Create(createValues);
        }
    }
}
