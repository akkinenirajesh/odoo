csharp
public partial class MailResPartner {
    public void ComputeContactAddressInline() {
        // Replace any successive \n with a single comma
        this.ContactAddressInline = Regex.Replace(this.ContactAddress, @"\n(\s|\n)*", ", ", RegexOptions.None).Trim(',').Trim();
    }

    public void ComputeNeedactionCount() {
        // compute the number of needaction of the current partner
        this.NeedactionCount = Env.ExecuteScalar<int>($@"
            SELECT COUNT(*)
            FROM MailNotification
            WHERE ResPartner = {this.Id}
            AND (IsRead = false OR IsRead IS NULL)
        ");
    }

    public Dictionary<string, object> MailPartnerFormat(List<string> fields = null) {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["Id"] = this.Id;
        if (fields == null || fields.Contains("Name")) {
            data["Name"] = this.Name;
        }
        if (fields == null || fields.Contains("Email")) {
            data["Email"] = this.Email;
        }
        if (fields == null || fields.Contains("Active")) {
            data["Active"] = this.Active;
        }
        if (fields == null || fields.Contains("ImStatus")) {
            data["ImStatus"] = this.ImStatus;
        }
        if (fields == null || fields.Contains("IsCompany")) {
            data["IsCompany"] = this.IsCompany;
        }
        if (fields == null || fields.Contains("WriteDate")) {
            data["WriteDate"] = DateTime.Now;
        }
        if (fields == null || fields.Contains("User")) {
            var users = this.User.Where(u => !u.Share).ToList();
            var mainUser = users.FirstOrDefault() ?? this.User.FirstOrDefault();
            if (mainUser != null) {
                data["UserId"] = mainUser.Id;
                data["IsInternalUser"] = !mainUser.Share;
            }
        }
        if (!Env.User.IsInternal()) {
            data.Remove("Email");
        }
        data["Type"] = "partner";
        return data;
    }

    public List<object> GetMentionSuggestions(string search, int limit = 8) {
        // Return 'limit'-first partners' such that the name or email matches a 'search' string.
        // Prioritize partners that are also (internal) users, and then extend the research to all partners.
        // The return format is a list of partner data (as per returned by `mail_partner_format()`).
        var domain = GetMentionSuggestionsDomain(search);
        var partners = SearchMentionSuggestions(domain, limit);
        return partners.Select(p => p.MailPartnerFormat()).ToList();
    }

    private List<string> GetMentionSuggestionsDomain(string search) {
        // Domain for getting mention suggestions
        return new List<string> {
            $"OR({new List<string> { $"Name LIKE '%{search}%'", $"Email LIKE '%{search}%'" }.ToArray()})",
            $"Active = true"
        };
    }

    private List<MailResPartner> SearchMentionSuggestions(List<string> domain, int limit) {
        // Prioritize partners that are also (internal) users, and then extend the research to all partners.
        var domainIsUser = new List<string> { 
            $"User != null",
            $"User.Active = true",
            $"AND({domain.ToArray()})"
        };
        var priorityConditions = new List<List<string>> {
            new List<string> { domainIsUser.ToSql(), "PartnerShare = false" }, // Search partners that are internal users
            domainIsUser, // Search partners that are users
            domain // Search partners that are not users
        };
        var partners = new List<MailResPartner>();
        foreach (var condition in priorityConditions) {
            var remainingLimit = limit - partners.Count;
            if (remainingLimit <= 0) {
                break;
            }
            var query = Env.Search<MailResPartner>(condition.ToSql(), remainingLimit);
            partners.AddRange(query);
        }
        return partners;
    }

    public List<object> ImSearch(string name, int limit = 20, List<int> excludedIds = null) {
        // Search partner with a name and return its id, name and im_status.
        // Note : the user must be logged
        // :param name : the partner name to search
        // :param limit : the limit of result to return
        // :param excluded_ids : the ids of excluded partners
        if (excludedIds == null) {
            excludedIds = new List<int>();
        }
        var users = Env.Search<ResUsers>(
            $"Id != {Env.User.Id} AND Name LIKE '%{name}%' AND Active = true AND Share = false AND Partner NOT IN ({excludedIds.ToSql()})",
            limit
        );
        return users.Select(u => u.Partner.MailPartnerFormat()).ToList();
    }

    public (MailResPartner, MailGuest) GetCurrentPersona() {
        if (Env.User == null || Env.User.IsPublic()) {
            return (null, Env.Get<MailGuest>().GetGuestFromContext());
        }
        return (Env.User.Partner, Env.Get<MailGuest>());
    }
}
