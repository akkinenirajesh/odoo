csharp
public partial class Sms.MailNotification {
    public void ComputeSmsId()
    {
        if (this.NotificationType != "sms" || this.SmsIdInt == 0)
        {
            return;
        }
        var sms = Env.Model("Sms.Sms").Search(new List<object>() {
            new List<object>() { "Id", "in", new List<object>() { this.SmsIdInt } },
            new List<object>() { "ToDelete", "!=", true }
        });
        if (sms.Count > 0)
        {
            this.SmsId = sms[0];
        }
    }
}
