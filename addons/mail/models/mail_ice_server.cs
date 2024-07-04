C#
public partial class MailIceServer
{
    public MailIceServer() { }

    public List<Dictionary<string, string>> GetLocalIceServers()
    {
        // firefox has a hard cap of 5 ice servers
        var iceServers = Env.Model("Mail.MailIceServer").Search(limit: 5);
        var formattedIceServers = new List<Dictionary<string, string>>();
        foreach (var iceServer in iceServers)
        {
            var formattedIceServer = new Dictionary<string, string>();
            formattedIceServer.Add("urls", $"{iceServer.ServerType}:{iceServer.Uri}");
            if (!string.IsNullOrEmpty(iceServer.Username))
            {
                formattedIceServer.Add("username", iceServer.Username);
            }
            if (!string.IsNullOrEmpty(iceServer.Credential))
            {
                formattedIceServer.Add("credential", iceServer.Credential);
            }
            formattedIceServers.Add(formattedIceServer);
        }
        return formattedIceServers;
    }

    public List<Dictionary<string, string>> GetIceServers()
    {
        if (Env.ConfigParameter.GetParam("mail.use_twilio_rtc_servers") == "True")
        {
            (var accountSid, var authToken) = MailTools.Discuss.GetTwilioCredentials(Env);
            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Tokens.json";
                var response = Requests.Post(url, auth: (accountSid, authToken), timeout: 60);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var responseJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    if (responseJson.ContainsKey("ice_servers"))
                    {
                        return (List<Dictionary<string, string>>)responseJson["ice_servers"];
                    }
                }
            }
        }
        return GetLocalIceServers();
    }
}
