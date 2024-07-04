csharp
public partial class BaseResUsersSettings {
    public virtual BaseResUsers UserId { get; set; }

    public virtual IEnumerable<object> GetFieldsBlacklist() {
        return new List<object>();
    }

    public virtual BaseResUsersSettings FindOrCreateForUser(BaseResUsers user) {
        var settings = user.Sudo().ResUsersSettingsIds;
        if (settings == null) {
            settings = Env.Create<BaseResUsersSettings>(new Dictionary<string, object> { { "UserId", user.Id } });
        }
        return settings;
    }

    public virtual Dictionary<string, object> FormatSettings(List<string> fieldsToFormat = null) {
        var fieldsBlacklist = GetFieldsBlacklist();
        if (fieldsToFormat != null) {
            fieldsToFormat = fieldsToFormat.Where(f => !fieldsBlacklist.Contains(f)).ToList();
        } else {
            fieldsToFormat = this.GetType().GetProperties().Where(p => p.Name == "Id" || (!p.GetCustomAttributes(typeof(Automatic), false).Any() && !fieldsBlacklist.Contains(p.Name))).Select(p => p.Name).ToList();
        }
        var res = FormatSettings(fieldsToFormat);
        return res;
    }

    public virtual Dictionary<string, object> FormatSettings(List<string> fieldsToFormat) {
        var res = ReadFormat(fieldsToFormat)[0];
        if (fieldsToFormat.Contains("UserId")) {
            res["UserId"] = new Dictionary<string, object> { { "Id", UserId.Id } };
        }
        return res;
    }

    public virtual Dictionary<string, object> SetResUsersSettings(Dictionary<string, object> newSettings) {
        var changedSettings = new Dictionary<string, object>();
        foreach (var setting in newSettings.Keys) {
            if (this.GetType().GetProperty(setting) != null && newSettings[setting] != this.GetType().GetProperty(setting).GetValue(this)) {
                changedSettings[setting] = newSettings[setting];
            }
        }
        Write(changedSettings);
        var formated = FormatSettings(changedSettings.Keys.ToList().Append("Id").ToList());
        return formated;
    }
}
