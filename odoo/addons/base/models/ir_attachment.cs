csharp
public partial class IrAttachment {
    // all the model methods are written here.

    public void ComputeResName() {
        if (this.ResModel != null && this.ResID != 0) {
            var record = Env.GetModel(this.ResModel).Browse(this.ResID);
            this.ResName = record.Get<string>("DisplayName");
        } else {
            this.ResName = null;
        }
    }

    public void Migrate() {
        if (!Env.IsAdmin()) {
            throw new AccessError("Only administrators can execute this action.");
        }

        var domain = GetStorageDomain();
        var attachments = Env.GetModel("IrAttachment").Search(domain);
        foreach (var attachment in attachments) {
            attachment.Write(new Dictionary<string, object> {
                {"Raw", attachment.Raw},
                {"MimeType", attachment.MimeType}
            });
        }
    }

    public List<object> GetServingGroups() {
        return new List<object> { "Base.GroupSystem" };
    }

    public void ValidateAccess(string accessToken) {
        if (accessToken != null) {
            if (this.AccessToken != accessToken) {
                throw new AccessError("Invalid access token");
            }
        } else if (!this.Public && !Env.User.IsPortal()) {
            // Check the read access on the record linked to the attachment
            // eg: Allow to download an attachment on a task from /my/tasks/task_id
            this.Check("read");
        }
    }

    private List<object> GetStorageDomain() {
        var storage = Env.GetParam("ir_attachment.location", "file");
        switch (storage) {
            case "db":
                return new List<object> { ("StoreFileName", "!=", null) };
            case "file":
                return new List<object> { ("DbDatas", "!=", null) };
            default:
                throw new Exception($"Invalid storage location: {storage}");
        }
    }

    private void Check(string mode, Dictionary<string, object> values = null) {
        if (!Env.IsSuperuser()) {
            if (!Env.IsAdmin() || !Env.User.IsInternal()) {
                throw new AccessError("Sorry, you are not allowed to access this document.");
            }

            if (mode == "read" && this.Public) {
                return;
            }

            if (!Env.IsSystem() && this.ResID == 0 && Env.User.ID != this.CreateUID) {
                throw new AccessError("Sorry, you are not allowed to access this document.");
            }

            if (this.ResField != null && mode != "create" && mode != "unlink") {
                var field = Env.GetModel(this.ResModel).Get<Field>(this.ResField);
                if (!field.IsAccessible(Env)) {
                    throw new AccessError("Sorry, you are not allowed to access this document.");
                }
            }

            if (this.ResModel != null && this.ResID != 0) {
                var record = Env.GetModel(this.ResModel).Browse(this.ResID).Exists();
                if (record != null) {
                    record.CheckAccessRights(mode == "create" || mode == "unlink" ? "write" : mode);
                    record.CheckAccessRules(mode == "create" || mode == "unlink" ? "write" : mode);
                }
            }

            if (values != null && values.ContainsKey("ResModel") && values.ContainsKey("ResID")) {
                var record = Env.GetModel(values["ResModel"].ToString()).Browse(values["ResID"].ToString()).Exists();
                if (record != null) {
                    record.CheckAccessRights(mode == "create" || mode == "unlink" ? "write" : mode);
                    record.CheckAccessRules(mode == "create" || mode == "unlink" ? "write" : mode);
                }
            }
        }
    }

    private string GenerateAccessToken() {
        return Guid.NewGuid().ToString();
    }
}
