csharp
public partial class MassMailingSms.MailingList
{
    public virtual void ActionViewMailings()
    {
        if (Env.Context.ContainsKey("mailing_sms"))
        {
            var action = Env.Actions.Get("MassMailingSms.MailingMailingActionSms");
            action.Domain = new List<object> { new[] { "Id", "in", this.MailingIds.Select(m => m.Id).ToArray() } };
            action.Context = new Dictionary<string, object>()
            {
                { "DefaultMailingType", "sms" },
                { "DefaultContactListIds", new List<object> { this.Id } },
                { "MailingSms", true }
            };
            Env.ReturnAction(action);
        }
        else
        {
            var action = base.ActionViewMailings();
            Env.ReturnAction(action);
        }
    }

    public virtual void ActionViewContactsSms()
    {
        var action = this.ActionViewContacts();
        action.Context = new Dictionary<string, object>(action.Context) { { "SearchDefaultFilterValidSmsRecipient", true } };
        Env.ReturnAction(action);
    }

    public virtual void ActionSendMailingSms()
    {
        var view = Env.Ref("MassMailingSms.MailingMailingViewFormSms");
        var action = Env.Actions.Get("MassMailingSms.MailingMailingActionSms");
        action.Update(new Dictionary<string, object>()
        {
            { "Context", new Dictionary<string, object>()
                {
                    { "DefaultContactListIds", new List<object> { this.Id } },
                    { "DefaultModelId", Env.Models.Get("mailing.list").Id },
                    { "DefaultMailingType", "sms" }
                }
            },
            { "Target", "current" },
            { "ViewType", "form" },
            { "Views", new List<object> { new[] { view.Id, "form" } } }
        });
        Env.ReturnAction(action);
    }

    public virtual string GetContactStatisticsFields()
    {
        var values = base.GetContactStatisticsFields();
        values.Update(new Dictionary<string, string>()
        {
            { "ContactCountSms", @"
                SUM(CASE WHEN
                    (c.PhoneSanitized IS NOT NULL
                    AND COALESCE(r.OptOut,FALSE) = FALSE
                    AND bl_sms.id IS NULL)
                THEN 1 ELSE 0 END) AS ContactCountSms" },
            { "ContactCountBlacklisted", @"
                SUM(CASE WHEN (bl.id IS NOT NULL OR bl_sms.id IS NOT NULL)
                THEN 1 ELSE 0 END) AS ContactCountBlacklisted" }
        });
        return values;
    }

    public virtual string GetContactStatisticsJoins()
    {
        return base.GetContactStatisticsJoins() + @"
            LEFT JOIN phone_blacklist bl_sms ON c.PhoneSanitized = bl_sms.number and bl_sms.active
        ";
    }

    public virtual List<object> GetOptOutListSms(MassMailingSms.Mailing mailing)
    {
        var subscriptions = this.SubscriptionIds != null ? this.SubscriptionIds : mailing.ContactListIds != null ? mailing.ContactListIds.SubscriptionIds : null;
        var optOutContacts = subscriptions.Where(s => s.OptOut).Select(s => s.ContactId);
        var optInContacts = subscriptions.Where(s => !s.OptOut).Select(s => s.ContactId);
        return optOutContacts.Except(optInContacts).ToList();
    }
}
