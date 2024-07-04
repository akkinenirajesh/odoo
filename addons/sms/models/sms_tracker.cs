csharp
public partial class SmsTracker {
    public string SmsUuid { get; set; }
    public Mail.Notification MailNotificationId { get; set; }
    public SmsTracker() { }

    public void ActionUpdateFromProviderError(string ProviderError) {
        string failureReason = false;
        string failureType = $"sms_{ProviderError}";
        string errorStatus = null;

        if (!Env.Get("sms.sms").DeliveryErrors.Contains(failureType)) {
            failureType = "unknown";
            failureReason = ProviderError;
        } else if (Env.Get("sms.sms").BounceDeliveryErrors.Contains(failureType)) {
            errorStatus = "bounce";
        }

        UpdateSmsNotifications(errorStatus ?? "exception", failureType: failureType, failureReason: failureReason);
    }

    public void ActionUpdateFromSmsState(string SmsState, string FailureType = false, string FailureReason = false) {
        string notificationStatus = Env.Get("Sms.SmsStateToNotificationStatus").GetOptionValue(SmsState);
        UpdateSmsNotifications(notificationStatus, failureType: FailureType, failureReason: FailureReason);
    }

    private void UpdateSmsNotifications(string NotificationStatus, string FailureType = false, string FailureReason = false) {
        List<string> notificationsStatusesToIgnore = new List<string>() {
            "canceled", "process", "pending", "sent"
        };

        switch (NotificationStatus) {
            case "ready":
                notificationsStatusesToIgnore = new List<string>() { "ready", "process", "pending", "sent" };
                break;
            case "process":
                notificationsStatusesToIgnore = new List<string>() { "process", "pending", "sent" };
                break;
            case "pending":
                notificationsStatusesToIgnore = new List<string>() { "pending", "sent" };
                break;
            case "bounce":
                notificationsStatusesToIgnore = new List<string>() { "bounce", "sent" };
                break;
            case "sent":
                notificationsStatusesToIgnore = new List<string>() { "sent" };
                break;
            case "exception":
                notificationsStatusesToIgnore = new List<string>() { "exception" };
                break;
        }

        List<Mail.Notification> notifications = this.MailNotificationId.Where(n => !notificationsStatusesToIgnore.Contains(n.NotificationStatus)).ToList();

        if (notifications.Any()) {
            notifications.ForEach(n => {
                n.NotificationStatus = NotificationStatus;
                n.FailureType = FailureType;
                n.FailureReason = FailureReason;
            });

            if (!Env.Context.ContainsKey("sms_skip_msg_notification")) {
                notifications.ForEach(n => n.MailMessageId._NotifyMessageNotificationUpdate());
            }
        }
    }
}
