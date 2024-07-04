csharp
public partial class MailBlacklist {
    public MailBlacklist() { }

    public MailBlacklist(int id) { }

    public MailBlacklist(Dictionary<string, object> values) { }

    public void Create(Dictionary<string, object> values) {
        // First of all, extract values to ensure emails are really unique (and don't modify values in place)
        List<Dictionary<string, object>> newValues = new List<Dictionary<string, object>>();
        List<string> allEmails = new List<string>();
        foreach (Dictionary<string, object> value in values) {
            string email = OdooTools.EmailNormalize(value["Email"].ToString());
            if (string.IsNullOrEmpty(email)) {
                throw new Exception($"Invalid email address “{value["Email"]}”");
            }
            if (allEmails.Contains(email)) {
                continue;
            }
            allEmails.Add(email);
            newValues.Add(new Dictionary<string, object>(value) { { "Email", email } });
        }

        // To avoid crash during import due to unique email, return the existing records if any
        List<Dictionary<string, object>> toCreate = new List<Dictionary<string, object>>();
        Dictionary<string, int> blEntries = new Dictionary<string, int>();
        if (newValues.Count > 0) {
            string sql = "SELECT email, id FROM mail_blacklist WHERE email = ANY(@emails)";
            List<string> emails = newValues.Select(v => v["Email"].ToString()).ToList();
            List<object[]> result = Env.ExecuteSql(sql, new { emails = emails });
            blEntries = result.ToDictionary(r => r[0].ToString(), r => (int)r[1]);
            toCreate = newValues.Where(v => !blEntries.ContainsKey(v["Email"].ToString())).ToList();
        }

        // TODO DBE Fixme : reorder ids according to incoming ids.
        List<MailBlacklist> results = Env.Create<MailBlacklist>(toCreate);
        return Env.Browse<MailBlacklist>(blEntries.Values.ToList()).Concat(results).ToList();
    }

    public void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Email")) {
            values["Email"] = OdooTools.EmailNormalize(values["Email"].ToString());
        }
        base.Write(values);
    }

    public List<MailBlacklist> Search(List<object> domain) {
        // Override _search in order to grep search on email field and make it lower-case and sanitized
        List<object> normalizedDomain = domain.Select(item => {
            if (item is List<object> && ((List<object>)item)[0] == "Email" && ((List<object>)item)[2] is string) {
                string normalized = OdooTools.EmailNormalize(((List<object>)item)[2].ToString());
                if (!string.IsNullOrEmpty(normalized)) {
                    return new List<object> { "Email", ((List<object>)item)[1], normalized };
                }
            }
            return item;
        }).ToList();

        return base.Search(normalizedDomain);
    }

    public MailBlacklist Add(string email, string message = null) {
        string normalized = OdooTools.EmailNormalize(email);
        MailBlacklist record = Env.Browse<MailBlacklist>().WithContext(new { active_test = false }).Search(new List<object> { ["Email", "=", normalized] }).FirstOrDefault();
        if (record != null) {
            if (!string.IsNullOrEmpty(message)) {
                record._TrackSetLogMessage(message);
            }
            record.ActionUnarchive();
        } else {
            record = Env.Create<MailBlacklist>(new Dictionary<string, object> { { "Email", email } });
            if (!string.IsNullOrEmpty(message)) {
                record.WithContext(new { mail_create_nosubscribe = true }).MessagePost(
                    new Dictionary<string, object> {
                        { "body", message },
                        { "subtype_xmlid", "mail.mt_note" }
                    }
                );
            }
        }
        return record;
    }

    public MailBlacklist Remove(string email, string message = null) {
        string normalized = OdooTools.EmailNormalize(email);
        MailBlacklist record = Env.Browse<MailBlacklist>().WithContext(new { active_test = false }).Search(new List<object> { ["Email", "=", normalized] }).FirstOrDefault();
        if (record != null) {
            if (!string.IsNullOrEmpty(message)) {
                record._TrackSetLogMessage(message);
            }
            record.ActionArchive();
        } else {
            record = Env.Create<MailBlacklist>(new Dictionary<string, object> { { "Email", email }, { "Active", false } });
            if (!string.IsNullOrEmpty(message)) {
                record.WithContext(new { mail_create_nosubscribe = true }).MessagePost(
                    new Dictionary<string, object> {
                        { "body", message },
                        { "subtype_xmlid", "mail.mt_note" }
                    }
                );
            }
        }
        return record;
    }

    public Dictionary<string, object> MailActionBlacklistRemove() {
        return new Dictionary<string, object> {
            { "name", "Are you sure you want to unblacklist this email address?" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "mail.blacklist.remove" },
            { "target", "new" },
            { "context", new Dictionary<string, object> { { "dialog_size", "medium" } } }
        };
    }

    public void ActionAdd() {
        this.Add(this.Email);
    }

    // Private methods or properties can be defined here
    private void _TrackSetLogMessage(string message) { }
    private void ActionArchive() { }
    private void ActionUnarchive() { }
    private void MessagePost(Dictionary<string, object> values) { }
}
