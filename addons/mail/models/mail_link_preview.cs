C#
public partial class MailLinkPreview {
    public void CreateFromMessageAndNotify(Mail.Message message) {
        if (tools.IsHtmlEmpty(message.Body)) {
            return;
        }
        var urls = new OrderedSet(html.Fromstring(message.Body).XPath("//a[not(@data-oe-model)]/@href"));
        var linkPreviews = Env.GetModel("Mail.LinkPreview");
        var requestsSession = new requests.Session();
        var linkPreviewValues = new List<Dictionary<string, object>>();
        var linkPreviewsByUrl = urls.ToDictionary(url => url, url => message.LinkPreviewIds.FirstOrDefault(p => p.SourceUrl == url));
        foreach (var url in urls) {
            if (linkPreviewsByUrl.ContainsKey(url)) {
                var preview = linkPreviewsByUrl[url];
                if (!preview.IsHidden) {
                    linkPreviews += preview;
                }
                linkPreviewsByUrl.Remove(url);
                continue;
            }
            var preview = link_preview.GetLinkPreviewFromUrl(url, requestsSession);
            if (preview != null) {
                preview["MessageId"] = message.Id;
                linkPreviewValues.Add(preview);
            }
            if (linkPreviewValues.Count + linkPreviews.Count > 5) {
                break;
            }
        }
        foreach (var unusedPreview in linkPreviewsByUrl.Values) {
            unusedPreview.UnlinkAndNotify();
        }
        if (linkPreviewValues.Count > 0) {
            linkPreviews += linkPreviews.Create(linkPreviewValues);
        }
        if (linkPreviews.Count > 0) {
            Env.GetModel("bus.bus")._Sendone(message._BusNotificationTarget(), "mail.record/insert", new Dictionary<string, object> {
                { "Message", new Dictionary<string, object> {
                    { "linkPreviews", linkPreviews.OrderBy(p => urls.IndexOf(p.SourceUrl))._LinkPreviewFormat() },
                    { "id", message.Id }
                }}
            });
        }
    }

    public void HideAndNotify() {
        if (this == null) {
            return;
        }
        var notifications = this.Select(linkPreview => new Tuple<string, string, Dictionary<string, object>>(
            linkPreview.MessageId._BusNotificationTarget(),
            "mail.record/insert",
            new Dictionary<string, object> {
                { "Message", new Dictionary<string, object> {
                    { "linkPreviews", new List<object> { ("DELETE", new Dictionary<string, object> { { "id", linkPreview.Id } }) } },
                    { "id", linkPreview.MessageId.Id }
                }}
            }
        )).ToList();
        this.IsHidden = true;
        Env.GetModel("bus.bus")._Sendmany(notifications);
    }

    public void UnlinkAndNotify() {
        if (this == null) {
            return;
        }
        var notifications = this.Select(linkPreview => new Tuple<string, string, Dictionary<string, object>>(
            linkPreview.MessageId._BusNotificationTarget(),
            "mail.record/insert",
            new Dictionary<string, object> {
                { "Message", new Dictionary<string, object> {
                    { "linkPreviews", new List<object> { ("DELETE", new Dictionary<string, object> { { "id", linkPreview.Id } }) } },
                    { "id", linkPreview.MessageId.Id }
                }}
            }
        )).ToList();
        Env.GetModel("bus.bus")._Sendmany(notifications);
        this.Unlink();
    }

    public bool IsLinkPreviewEnabled() {
        var linkPreviewThrottle = Convert.ToInt32(Env.GetModel("ir.config_parameter").GetParam("mail.link_preview_throttle", "99"));
        return linkPreviewThrottle > 0;
    }

    public Dictionary<string, object> SearchOrCreateFromUrl(string url) {
        var lifetime = Convert.ToInt32(Env.GetModel("ir.config_parameter").GetParam("mail.mail_link_preview_lifetime_days", "3"));
        var preview = Env.GetModel("Mail.LinkPreview").Search(new List<object> {
            new object[] { "SourceUrl", "=", url },
            new object[] { "CreateDate", ">=", fields.Datetime.Now - timedelta.FromDays(lifetime) }
        }, new Dictionary<string, object> { { "order", "CreateDate DESC" }, { "limit", 1 } });
        if (preview == null) {
            var previewValues = link_preview.GetLinkPreviewFromUrl(url);
            if (previewValues == null) {
                return null;
            }
            preview = Env.GetModel("Mail.LinkPreview").Create(previewValues);
        }
        return preview._LinkPreviewFormat()[0];
    }

    public List<Dictionary<string, object>> _LinkPreviewFormat() {
        return this.Select(preview => new Dictionary<string, object> {
            { "id", preview.Id },
            { "message", new Dictionary<string, object> { { "id", preview.MessageId.Id } } },
            { "ImageMimeType", preview.ImageMimeType },
            { "OgDescription", preview.OgDescription },
            { "OgImage", preview.OgImage },
            { "OgMimeType", preview.OgMimeType },
            { "OgTitle", preview.OgTitle },
            { "OgType", preview.OgType },
            { "OgSiteName", preview.OgSiteName },
            { "SourceUrl", preview.SourceUrl }
        }).ToList();
    }

    public void GcMailLinkPreview() {
        var lifetime = Convert.ToInt32(Env.GetModel("ir.config_parameter").GetParam("mail.mail_link_preview_lifetime_days", "3"));
        Env.GetModel("Mail.LinkPreview").Search(new List<object> {
            new object[] { "MessageId", "=", null },
            new object[] { "CreateDate", "<", fields.Datetime.Now - timedelta.FromDays(lifetime) }
        }, new Dictionary<string, object> { { "order", "CreateDate ASC" }, { "limit", 1000 } }).Unlink();
    }
}
