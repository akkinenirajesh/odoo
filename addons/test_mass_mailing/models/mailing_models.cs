C#
public partial class MailingCustomer {
    public void ComputeEmailFrom() {
        if (this.EmailFrom != null || this.CustomerId == 0) {
            return;
        }

        this.EmailFrom = Env.GetModel("res.partner").GetById(this.CustomerId).GetPropertyValue<string>("Email");
    }

    public Dictionary<int, Dictionary<string, object>> GetDefaultRecipients() {
        var defaultRecipients = base.GetDefaultRecipients();
        if (this.CustomerId != 0) {
            defaultRecipients[this.Id] = new Dictionary<string, object> {
                { "EmailCc", false },
                { "EmailTo", false },
                { "PartnerIds", new List<int> { this.CustomerId } }
            };
        }
        return defaultRecipients;
    }
}

public partial class MailingSimple {
}

public partial class MailingUTM {
}

public partial class MailingBlacklist {
    public Dictionary<int, Dictionary<string, object>> GetDefaultRecipients() {
        var defaultRecipients = base.GetDefaultRecipients();
        if (this.CustomerId != 0) {
            defaultRecipients[this.Id] = new Dictionary<string, object> {
                { "EmailCc", false },
                { "EmailTo", false },
                { "PartnerIds", new List<int> { this.CustomerId } }
            };
        }
        return defaultRecipients;
    }
}

public partial class MailingOptOut {
    public List<string> GetOptOutList(Mailing mailing) {
        var resIds = mailing.GetRecipients();
        var optOutContacts = Env.GetModel("TestMassMailing.MailingOptOut").Search(new List<object> {
            new List<object> { "Id", "in", resIds },
            new List<object> { "OptOut", "=", true }
        }).Map(record => record.GetPropertyValue<string>("EmailNormalized"));
        return optOutContacts;
    }

    public Dictionary<int, Dictionary<string, object>> GetDefaultRecipients() {
        var defaultRecipients = base.GetDefaultRecipients();
        if (this.CustomerId != 0) {
            defaultRecipients[this.Id] = new Dictionary<string, object> {
                { "EmailCc", false },
                { "EmailTo", false },
                { "PartnerIds", new List<int> { this.CustomerId } }
            };
        }
        return defaultRecipients;
    }
}

public partial class MailingTestPartner {
}

public partial class MailingPerformance {
}

public partial class MailingPerformanceBL {
}
