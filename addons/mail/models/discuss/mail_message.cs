csharp
public partial class MailMessage {
    public bool ValidateAccessForCurrentPersona(string operation) {
        if (this == null) {
            return false;
        }

        if (Env.User.IsPublic()) {
            var guest = Env.Get<Mail.Guest>().GetGuestFromContext();
            // sudo: mail.guest - current guest can read channels they are member of
            return guest != null && this.Model == "discuss.channel" && guest.ChannelIds.Contains(this.ResId);
        }

        return base.ValidateAccessForCurrentPersona(operation);
    }

    public Dictionary<string, object> MessageFormatExtras(bool formatReply) {
        var vals = base.MessageFormatExtras(formatReply);
        if (formatReply && this.Model == "discuss.channel" && this.ParentMessageId > 0) {
            vals["ParentMessage"] = Env.Get<Mail.MailMessage>().Browse(this.ParentMessageId).MessageFormat(formatReply: false)[0];
        }

        return vals;
    }

    public object BusNotificationTarget() {
        if (this.Model == "discuss.channel" && this.ResId > 0) {
            return Env.Get<Discuss.Channel>().Browse(this.ResId);
        }

        var guest = Env.Get<Mail.Guest>().GetGuestFromContext();
        if (Env.User.IsPublic() && guest != null) {
            return guest;
        }

        return base.BusNotificationTarget();
    }
}
