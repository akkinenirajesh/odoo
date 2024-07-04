C#
public partial class Portal.MailMessage {
    public List<Attachment.Attachment> AttachmentIds { get; set; }
    public string AuthorAvatarUrl { get; set; }
    public Res.Users AuthorId { get; set; }
    public string Body { get; set; }
    public DateTime Date { get; set; }
    public int Id { get; set; }
    public bool IsInternal { get; set; }
    public bool IsMessageSubtypeNote { get; set; }
    public string PublishedDateStr { get; set; }
    public Mail.MessageSubtype SubtypeId { get; set; }

    public List<Dictionary<string, object>> PortalMessageFormat(Dictionary<string, object> options) {
        Env.CheckAccessRule("read");
        return _PortalMessageFormat(
            _PortalGetDefaultFormatPropertiesNames(options),
            options
        );
    }

    private List<string> _PortalGetDefaultFormatPropertiesNames(Dictionary<string, object> options) {
        return new List<string>() {
            "AttachmentIds",
            "AuthorAvatarUrl",
            "AuthorId",
            "Body",
            "Date",
            "Id",
            "IsInternal",
            "IsMessageSubtypeNote",
            "PublishedDateStr",
            "SubtypeId",
        };
    }

    private List<Dictionary<string, object>> _PortalMessageFormat(List<string> propertiesNames, Dictionary<string, object> options) {
        Dictionary<int, List<Dictionary<string, object>>> messageToAttachments = new Dictionary<int, List<Dictionary<string, object>>>();
        if (propertiesNames.Contains("AttachmentIds")) {
            propertiesNames.Remove("AttachmentIds");
            var attachmentsSudo = this.Env.Sudo().GetCollection<Attachment.Attachment>("AttachmentIds");
            attachmentsSudo.GenerateAccessToken();
            var relatedAttachments = attachmentsSudo.Read(
                new List<string>() { "AccessToken", "Checksum", "Id", "Mimetype", "Name", "ResId", "ResModel" }
            ).ToDictionary(
                x => (int)x["Id"],
                x => new Dictionary<string, object>() {
                    { "AccessToken", x["AccessToken"] },
                    { "Checksum", x["Checksum"] },
                    { "Id", x["Id"] },
                    { "Mimetype", x["Mimetype"] },
                    { "Name", x["Name"] },
                    { "ResId", x["ResId"] },
                    { "ResModel", x["ResModel"] }
                }
            );
            messageToAttachments = this.Env.Sudo().GetCollection<Portal.MailMessage>().ToDictionary(
                x => x.Id,
                x => x.AttachmentIds.Select(
                    att => _PortalMessageFormatAttachments(relatedAttachments[att.Id])
                ).ToList()
            );
        }

        var fnames = propertiesNames.Where(x => this.Env.GetModel<Portal.MailMessage>().GetField(x) != null).ToList();
        var valsList = this.Env.Sudo().GetCollection<Portal.MailMessage>().Read(fnames);

        var noteId = this.Env.GetModel<Ir.ModelData>()._XmlidToResId("mail.mt_note");
        foreach (var message in this.Env.Sudo().GetCollection<Portal.MailMessage>()) {
            foreach (var values in valsList) {
                if (messageToAttachments.ContainsKey(message.Id)) {
                    values["AttachmentIds"] = messageToAttachments[message.Id];
                }
                if (propertiesNames.Contains("AuthorAvatarUrl")) {
                    if (options.ContainsKey("token")) {
                        values["AuthorAvatarUrl"] = $"/mail/avatar/mail.message/{message.Id}/author_avatar/50x50?access_token={options["token"]}";
                    } else if (options.ContainsKey("hash") && options.ContainsKey("pid")) {
                        values["AuthorAvatarUrl"] = $"/mail/avatar/mail.message/{message.Id}/author_avatar/50x50?_hash={options["hash"]}&pid={options["pid"]}";
                    } else {
                        values["AuthorAvatarUrl"] = $"/web/image/mail.message/{message.Id}/author_avatar/50x50";
                    }
                }
                if (propertiesNames.Contains("IsMessageSubtypeNote")) {
                    values["IsMessageSubtypeNote"] = ((int)values["SubtypeId"] == noteId);
                }
                if (propertiesNames.Contains("PublishedDateStr")) {
                    values["PublishedDateStr"] = (DateTime)values["Date"] != DateTime.MinValue ? (DateTime)values["Date"].ToString() : "";
                }
            }
        }
        return valsList;
    }

    private Dictionary<string, object> _PortalMessageFormatAttachments(Dictionary<string, object> attachmentValues) {
        var safari = this.Env.Request.Httprequest.UserAgent.Browser == "safari";
        attachmentValues["Filename"] = attachmentValues["Name"];
        attachmentValues["Mimetype"] = safari && attachmentValues["Mimetype"].ToString().Contains("video") ?
            "application/octet-stream" : attachmentValues["Mimetype"].ToString();
        return attachmentValues;
    }
}
