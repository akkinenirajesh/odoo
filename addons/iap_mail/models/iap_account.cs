csharp
public partial class IapAccount
{
    public void SendSuccessNotification(string message, string title = null)
    {
        SendStatusNotification(message, "success", title);
    }

    public void SendErrorNotification(string message, string title = null)
    {
        SendStatusNotification(message, "danger", title);
    }

    private void SendStatusNotification(string message, string status, string title = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["message"] = message,
            ["type"] = status
        };

        if (title != null)
        {
            parameters["title"] = title;
        }

        Env.Get<BusBus>().SendOne(Env.User.Partner, "iap_notification", parameters);
    }

    public void SendNoCreditNotification(string serviceName, string title)
    {
        var parameters = new Dictionary<string, object>
        {
            ["title"] = title,
            ["type"] = "no_credit",
            ["get_credits_url"] = Env.Get<IapAccount>().GetCreditsUrl(serviceName)
        };

        Env.Get<BusBus>().SendOne(Env.User.Partner, "iap_notification", parameters);
    }
}
