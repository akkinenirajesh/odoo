csharp
public partial class MailMail {
    public MailMail() {
    }

    public void Create(List<Dictionary<string, object>> valuesList) {
        var mails = Env.Call("super", "create", valuesList);
        foreach (var mail in mails) {
            var values = valuesList[mails.IndexOf(mail)];
            if (values.ContainsKey("MailingTraceIds")) {
                Env.Call("MailingTraceIds", "Write", new Dictionary<string, object>() { {"MessageId", mail.Get("MessageId")} });
            }
        }
    }

    public string GetTrackingUrl() {
        var token = GenerateMailRecipientToken(this.Get("Id"));
        return Env.Call<string>("werkzeug.urls", "url_join", Env.Get("base_url"), $"mail/track/{this.Get("Id")}/{token}/blank.gif");
    }

    public string GenerateMailRecipientToken(object mailId) {
        return Env.Call<string>("tools", "hmac", Env.Create("su", true), "mass_mailing-mail_mail-open", mailId);
    }

    public string PrepareOutgoingBody() {
        var body = Env.Call("super", "PrepareOutgoingBody");
        if (body != null && this.Get("MailingId") != null && this.Get("MailingTraceIds") != null) {
            foreach (var match in new HashSet<string>(Env.Call<List<string>>("re", "findall", Env.Call("tools", "URL_REGEX"), body))) {
                var href = match.Split("|")[0];
                var url = match.Split("|")[1];

                var parsed = Env.Call<object>("werkzeug.urls", "url_parse", url, "http");

                if (Env.Call<string>(parsed, "scheme").StartsWith("http") && Env.Call<string>(parsed, "path").StartsWith("/r/")) {
                    var newHref = href.Replace(url, $"{url}/m/{this.Get("MailingTraceIds")[0].Get("Id")}");
                    body = body.Replace(href, newHref);
                }
            }

            var trackingUrl = GetTrackingUrl();
            body = Env.Call<string>("tools", "append_content_to_html", body, $"<img src=\"{trackingUrl}\"/>", false);
        }
        return body;
    }

    public List<Dictionary<string, object>> PrepareOutgoingList(object mailServer = null, object recipientsFollowerStatus = null) {
        var emailList = Env.Call<List<Dictionary<string, object>>>("super", "PrepareOutgoingList", mailServer, recipientsFollowerStatus);
        if (this.Get("ResId") == null || this.Get("MailingId") == null) {
            return emailList;
        }

        var baseUrl = Env.Call<string>(this.Get("MailingId"), "GetBaseUrl");
        foreach (var emailValues in emailList) {
            if (emailValues["EmailTo"] == null) {
                continue;
            }

            var emails = Env.Call<List<string>>("tools", "email_split", emailValues["EmailTo"][0]);
            var emailTo = emails.Count > 0 ? emails[0] : null;
            var unsubscribeUrl = Env.Call<string>(this.Get("MailingId"), "GetUnsubscribeUrl", emailTo, this.Get("ResId"));
            var unsubscribeOneclickUrl = Env.Call<string>(this.Get("MailingId"), "GetUnsubscribeOneclickUrl", emailTo, this.Get("ResId"));
            var viewUrl = Env.Call<string>(this.Get("MailingId"), "GetViewUrl", emailTo, this.Get("ResId"));

            if (Env.Call<bool>("tools", "is_html_empty", emailValues["body"])) {
                if (baseUrl + "/unsubscribe_from_list" in emailValues["body"] && !Env.Get("mailing_test_mail")) {
                    emailValues["body"] = emailValues["body"].Replace(baseUrl + "/unsubscribe_from_list", unsubscribeUrl);
                }
                if (baseUrl + "/view" in emailValues["body"]) {
                    emailValues["body"] = emailValues["body"].Replace(baseUrl + "/view", viewUrl);
                }
            }

            emailValues["headers"].AddOrUpdate("List-Unsubscribe", $"<{unsubscribeOneclickUrl}>");
            emailValues["headers"].AddOrUpdate("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
            emailValues["headers"].AddOrUpdate("Precedence", "list");
            emailValues["headers"].AddOrUpdate("X-Auto-Response-Suppress", "OOF");
        }
        return emailList;
    }

    public void PostprocessSentMessage(List<object> successPids, object failureReason = null, object failureType = null) {
        if (failureType != null) {
            Env.Call("MailingTraceIds", "SetFailed", "failureType", failureType);
        } else {
            Env.Call("MailingTraceIds", "SetSent");
        }
        Env.Call("super", "PostprocessSentMessage", successPids, failureReason, failureType);
    }
}
