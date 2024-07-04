csharp
public partial class MailPush 
{
    public void PushNotificationToEndpoint(int batchSize = 50)
    {
        var webPushNotificationsSudo = Env.GetModel("Mail.MailPush").Search(new Dictionary<string, object>(), new List<string> { "MailPushDeviceId", "Payload" }, batchSize);
        if (webPushNotificationsSudo.Count == 0)
        {
            return;
        }

        var irParameterSudo = Env.GetModel("Ir.ConfigParameter");
        var vapidPrivateKey = irParameterSudo.GetParameter("mail.web_push_vapid_private_key");
        var vapidPublicKey = irParameterSudo.GetParameter("mail.web_push_vapid_public_key");
        if (string.IsNullOrEmpty(vapidPrivateKey) || string.IsNullOrEmpty(vapidPublicKey))
        {
            return;
        }

        var session = new System.Net.Http.HttpClient();
        var devicesToUnlink = new HashSet<int>();

        var devices = webPushNotificationsSudo.GroupBy(x => x.MailPushDeviceId.Id).ToDictionary(g => g.Key, g => g.FirstOrDefault());
        foreach (var webPushNotificationSudo in webPushNotificationsSudo)
        {
            if (devicesToUnlink.Contains(webPushNotificationSudo.MailPushDeviceId.Id))
            {
                continue;
            }

            try
            {
                Tools.WebPush.PushToEndPoint(
                    baseUrl: GetBaseUrl(),
                    device: new Tools.WebPush.Device
                    {
                        Id = devices[webPushNotificationSudo.MailPushDeviceId.Id].Id,
                        EndPoint = devices[webPushNotificationSudo.MailPushDeviceId.Id].EndPoint,
                        Keys = devices[webPushNotificationSudo.MailPushDeviceId.Id].Keys
                    },
                    payload: webPushNotificationSudo.Payload,
                    vapidPrivateKey: vapidPrivateKey,
                    vapidPublicKey: vapidPublicKey,
                    session: session);
            }
            catch (Tools.WebPush.DeviceUnreachableError)
            {
                devicesToUnlink.Add(devices[webPushNotificationSudo.MailPushDeviceId.Id].Id);
            }
        }

        webPushNotificationsSudo.ForEach(x => x.Unlink());

        if (devicesToUnlink.Any())
        {
            Env.GetModel("Mail.MailPushDevice").Browse(devicesToUnlink.ToList()).ForEach(x => x.Unlink());
        }

        if (Env.GetModel("Mail.MailPush").SearchCount(new Dictionary<string, object>()) > 0)
        {
            Env.GetReference("mail.ir_cron_web_push_notification").Trigger();
        }
    }

    private string GetBaseUrl()
    {
        // Implement your logic to get the base URL here.
        return "https://your-odoo-server.com";
    }
}
