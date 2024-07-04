csharp
public partial class MailNotification {
    public MailNotification() { }

    public void Init() {
        Env.Execute("CREATE INDEX IF NOT EXISTS mail_notification_res_partner_id_is_read_notification_status_mail_message_id ON mail_notification (res_partner_id, is_read, notification_status, mail_message_id)");
        Env.Execute("CREATE INDEX IF NOT EXISTS mail_notification_author_id_notification_status_failure ON mail_notification (author_id, notification_status) WHERE notification_status IN ('bounce', 'exception')");
        Env.Execute($"CREATE UNIQUE INDEX IF NOT EXISTS unique_mail_message_id_res_partner_id_if_set ON {this.GetTableName()} (mail_message_id, res_partner_id) WHERE res_partner_id IS NOT NULL");
    }

    public MailNotification Create(Dictionary<string, object> vals) {
        var messages = Env.Model("mail.message").Browse(vals["MailMessageId"]);
        messages.CheckAccessRights("read");
        messages.CheckAccessRule("read");
        if (vals.ContainsKey("IsRead") && (bool)vals["IsRead"]) {
            vals["ReadDate"] = DateTime.Now;
        }
        return Env.Model("mail.mailnotification").Create(vals);
    }

    public void Write(Dictionary<string, object> vals) {
        if ((vals.ContainsKey("MailMessageId") || vals.ContainsKey("ResPartnerId")) && !Env.IsAdmin()) {
            throw new Exception("Can not update the message or recipient of a notification.");
        }
        if (vals.ContainsKey("IsRead") && (bool)vals["IsRead"]) {
            vals["ReadDate"] = DateTime.Now;
        }
        Env.Model("mail.mailnotification").Browse(this.Id).Write(vals);
    }

    public void GCNotifications(int maxAgeDays = 180) {
        var domain = new Dictionary<string, object> {
            {"IsRead", true},
            {"ReadDate", DateTime.Now.AddDays(-maxAgeDays)},
            {"ResPartnerId.PartnerShare", false},
            {"NotificationStatus", new List<string> { "sent", "canceled" }}
        };
        Env.Model("mail.mailnotification").Search(domain).Unlink();
    }

    public string FormatFailureReason() {
        if (this.FailureType != "unknown") {
            return Env.OptionSet("Mail.FailureType").GetOption(this.FailureType).Name;
        } else {
            return $"Unknown error: {this.FailureReason ?? ""}";
        }
    }

    public List<MailNotification> FilteredForWebClient() {
        return this.Where(notif => notif.NotificationStatus in new List<string> {"bounce", "exception", "canceled"} || notif.ResPartnerId.PartnerShare || (notif.MailMessageId.Subtype.TrackRecipients ?? false)).ToList();
    }

    public List<Dictionary<string, object>> NotificationFormat() {
        return this.Select(notif => new Dictionary<string, object> {
            {"id", notif.Id},
            {"NotificationType", notif.NotificationType},
            {"NotificationStatus", notif.NotificationStatus},
            {"FailureType", notif.FailureType},
            {"Persona", notif.ResPartnerId != null ? new Dictionary<string, object> { 
                {"id", notif.ResPartnerId.Id}, 
                {"displayName", notif.ResPartnerId.DisplayName}, 
                {"type", "partner"} 
            } : null}
        }).ToList();
    }
}
