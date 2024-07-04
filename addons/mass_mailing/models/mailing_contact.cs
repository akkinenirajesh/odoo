csharp
public partial class MassMailingContact {

    public void ComputeName() {
        if (!string.IsNullOrEmpty(this.FirstName) || !string.IsNullOrEmpty(this.LastName)) {
            this.Name = string.Join(" ", new string[] { this.FirstName, this.LastName }.Where(n => !string.IsNullOrEmpty(n)).ToArray());
        }
    }

    public void ComputeOptOut() {
        if (Env.Context.ContainsKey("default_list_ids") && Env.Context["default_list_ids"] is List<int> listIds && listIds.Count == 1) {
            int activeListId = listIds[0];
            this.OptOut = this.SubscriptionIds.Where(l => l.ListId == activeListId).FirstOrDefault().OptOut;
        } else {
            this.OptOut = false;
        }
    }

    public List<MassMailingContact> Create(List<Dictionary<string, object>> valsList) {
        List<int> defaultListIds = Env.Context.ContainsKey("default_list_ids") && Env.Context["default_list_ids"] is List<int> ? (List<int>)Env.Context["default_list_ids"] : new List<int>();

        foreach (Dictionary<string, object> vals in valsList) {
            if (vals.ContainsKey("ListIds") && vals.ContainsKey("SubscriptionIds")) {
                throw new Exception("You should give either ListIds, either SubscriptionIds to create new contacts.");
            }

            if (defaultListIds.Count > 0) {
                if (!vals.ContainsKey("ListIds")) {
                    List<Dictionary<string, object>> subscriptionIds = vals.ContainsKey("SubscriptionIds") ? (List<Dictionary<string, object>>)vals["SubscriptionIds"] : new List<Dictionary<string, object>>();
                    List<int> currentListIds = subscriptionIds.Select(s => (int)s["ListId"]).ToList();
                    foreach (int listId in defaultListIds.Except(currentListIds)) {
                        subscriptionIds.Add(new Dictionary<string, object>() { { "ListId", listId } });
                    }
                    vals["SubscriptionIds"] = subscriptionIds;
                }
            }
        }

        return Env.WithContext(new Dictionary<string, object>() { { "default_list_ids", null } }).Create(valsList);
    }

    public MassMailingContact Copy(Dictionary<string, object> defaultValues = null) {
        if (Env.Context.ContainsKey("default_list_ids")) {
            Env = Env.WithContext(new Dictionary<string, object>() { { "default_list_ids", null } });
        }
        return base.Copy(defaultValues);
    }

    public (int Id, string DisplayName) NameCreate(string name) {
        (string name, string email) = Tools.ParseContactFromEmail(name);
        MassMailingContact contact = Env.Create(new Dictionary<string, object>() { { "Name", name }, { "Email", email } });
        return (contact.Id, contact.DisplayName);
    }

    public (int Id, string DisplayName) AddToList(string name, int listId) {
        (string name, string email) = Tools.ParseContactFromEmail(name);
        MassMailingContact contact = Env.Create(new Dictionary<string, object>() { { "Name", name }, { "Email", email }, { "ListIds", new List<int>() { listId } } });
        return (contact.Id, contact.DisplayName);
    }

    public Dictionary<int, Dictionary<string, string>> MessageGetDefaultRecipients() {
        return this.Select(r => new { Id = r.Id, Email = r.Email }).ToDictionary(
            r => r.Id, 
            r => new Dictionary<string, string>() { { "email_to", string.Join(",", Tools.EmailNormalizeAll(r.Email)) ?? r.Email }, { "email_cc", "" } }
        );
    }

    public Dictionary<string, object> ActionImport() {
        Dictionary<string, object> action = Env.Ref("mass_mailing.mailing_contact_import_action");
        Dictionary<string, object> context = Env.Context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        action["context"] = context;
        if (!context.ContainsKey("default_mailing_list_ids") && context.ContainsKey("from_mailing_list_ids")) {
            action["context"] = new Dictionary<string, object>() { { "default_mailing_list_ids", context["from_mailing_list_ids"] } };
        }
        return action;
    }

    public Dictionary<string, object> ActionAddToMailingList() {
        Dictionary<string, object> ctx = Env.Context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        ctx.Add("default_contact_ids", this.Select(c => c.Id).ToList());
        Dictionary<string, object> action = Env.Ref("mass_mailing.mailing_contact_to_list_action");
        action["view_mode"] = "form";
        action["target"] = "new";
        action["context"] = ctx;
        return action;
    }

    public List<Dictionary<string, object>> GetImportTemplates() {
        return new List<Dictionary<string, object>>() {
            new Dictionary<string, object>() {
                { "label", "Import Template for Mailing List Contacts" },
                { "template", "/mass_mailing/static/xls/mailing_contact.xls" }
            }
        };
    }

    public bool IsNameSplitActivated() {
        IrUiView view = Env.Ref("mass_mailing.mailing_contact_view_tree_split_name");
        return view != null && view.Active;
    }

    // other methods...

}
