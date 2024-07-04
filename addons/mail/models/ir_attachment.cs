csharp
public partial class Mail.IrAttachment {
    public void CheckAttachmentsAccess(List<string> attachmentTokens) {
        // This method relies on access rules/rights and therefore it should not be called from a sudo env.
        if (attachmentTokens == null || attachmentTokens.Count != this.Count) {
            throw new Exception("An access token must be provided for each attachment.");
        }

        foreach (var attachment in this) {
            try {
                var attachmentSudo = Env.GetModel("Mail.IrAttachment").Browse(attachment.Id).WithUser(Env.SuperUserId).Exists();
                if (!attachmentSudo) {
                    throw new Exception($"The attachment {attachment.Id} does not exist.");
                }

                if (!attachment.Check("write")) {
                    if (string.IsNullOrEmpty(attachmentTokens[attachment.Id]) ||
                        string.IsNullOrEmpty(attachmentSudo.AccessToken) ||
                        !attachmentSudo.AccessToken.Equals(attachmentTokens[attachment.Id]))
                    {
                        var messageSudo = Env.GetModel("Mail.Message").Browse(new List<int>() { attachment.Id }).WithUser(Env.SuperUserId);
                        if (messageSudo.Count == 0 || !messageSudo[0].IsCurrentUserOrGuestAuthor) {
                            throw new Exception($"The attachment {attachment.Id} does not exist or you do not have the rights to access it.");
                        }
                    }
                }
            } catch (Exception ex) {
                throw new Exception($"The attachment {attachment.Id} does not exist or you do not have the rights to access it.");
            }
        }
    }

    public void PostAddCreate(Dictionary<string, object> kwargs) {
        base.PostAddCreate(kwargs);
        RegisterAsMainAttachment(false);
    }

    public void RegisterAsMainAttachment(bool force) {
        var todo = this.Where(a => !string.IsNullOrEmpty(a.ResModel) && a.ResId > 0).ToList();
        if (todo.Count == 0) {
            return;
        }

        foreach (var group in todo.GroupBy(a => a.ResModel)) {
            var relatedRecords = Env.GetModel(group.Key).Browse(group.Select(a => a.ResId).ToList());
            if (!relatedRecords.HasMethod("_message_set_main_attachment_id")) {
                return;
            }

            for (int i = 0; i < relatedRecords.Count; i++) {
                try {
                    relatedRecords[i]._message_set_main_attachment_id(this[i], force);
                } catch (Exception ex) {
                    // Skip update if user cannot update record
                }
            }
        }
    }

    public void DeleteAndNotify(Mail.Message message) {
        if (message != null) {
            // sudo: mail.message - safe write just updating the date, because guests don't have the rights
            message.Write(new Dictionary<string, object>());
        }

        foreach (var attachment in this) {
            Env.GetModel("Bus.Bus").SendMany(new List<Tuple<string, string, Dictionary<string, object>>>() {
                new Tuple<string, string, Dictionary<string, object>>(
                    attachment.BusNotificationTarget(),
                    "ir.attachment/delete",
                    new Dictionary<string, object> {
                        { "id", attachment.Id },
                        { "message", message != null ? new Dictionary<string, object> {
                            { "id", message.Id },
                            { "write_date", message.WriteDate }
                        } : null }
                    })
            });
        }

        this.Unlink();
    }

    public string BusNotificationTarget() {
        return Env.User.Partner.Id;
    }

    public List<Dictionary<string, object>> AttachmentFormat() {
        return this.Select(attachment => new Dictionary<string, object> {
            { "checksum", attachment.Checksum },
            { "create_date", attachment.CreateDate },
            { "id", attachment.Id },
            { "filename", attachment.Name },
            { "name", attachment.Name },
            { "size", attachment.FileSize },
            { "res_name", attachment.ResName },
            { "mimetype", (Env.Request != null && Env.Request.UserAgent.Browser == "safari" && !string.IsNullOrEmpty(attachment.MimeType) && attachment.MimeType.Contains("video")) ? "application/octet-stream" : attachment.MimeType },
            { "thread", new Dictionary<string, object> {
                { "id", attachment.ResId },
                { "model", attachment.ResModel }
            }}
        }).ToList();
    }
}
