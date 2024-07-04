csharp
public partial class Mail.MailMessage
{
    public void ComputeHasSmsError()
    {
        var smsErrorFromNotification = Env.Ref<Mail.MailNotification>().Search(new[] {
            new Condition("NotificationType", "=", "sms"),
            new Condition("MailMessageId", "in", this.Id),
            new Condition("NotificationStatus", "=", "exception")
        });
        this.HasSmsError = smsErrorFromNotification.Select(x => x.MailMessageId).Contains(this.Id);
    }

    public List<object> MessageFormat(bool formatReply = true, Dictionary<string, object> msgVals = null, bool forCurrentUser = false)
    {
        var messageValues = base.MessageFormat(formatReply, msgVals, forCurrentUser);
        var allSmsNotifications = Env.Ref<Mail.MailNotification>().Search(new[] {
            new Condition("MailMessageId", "in", messageValues.Select(x => x["Id"])),
            new Condition("NotificationType", "=", "sms")
        });
        var msgIdToNotif = new Dictionary<int, List<Mail.MailNotification>>();
        foreach (var notif in allSmsNotifications)
        {
            if (!msgIdToNotif.ContainsKey(notif.MailMessageId.Id))
            {
                msgIdToNotif.Add(notif.MailMessageId.Id, new List<Mail.MailNotification>());
            }
            msgIdToNotif[notif.MailMessageId.Id].Add(notif);
        }

        foreach (var message in messageValues)
        {
            var customerSmsData = msgIdToNotif.ContainsKey((int)message["Id"]) ? msgIdToNotif[(int)message["Id"]].Select(notif => new object[] {
                notif.Id,
                notif.ResPartnerId.DisplayName ?? notif.SmsNumber,
                notif.NotificationStatus
            }).ToList() : new List<object>();
            message["SmsIds"] = customerSmsData;
        }
        return messageValues;
    }
}
