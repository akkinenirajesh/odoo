csharp
public partial class MassMailingList
{
    public override string ToString()
    {
        return $"{Name} ({ContactCount})";
    }

    public IrActionsAction ActionOpenImport()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.MailingContactImportAction");
        action.Context = new Dictionary<string, object>
        {
            ["default_mailing_list_ids"] = new[] { Id },
            ["default_subscription_ids"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["0"] = "create",
                    ["list_id"] = Id
                }
            }
        };
        return action;
    }

    public IrActionsAction ActionSendMailing()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.MailingMailingActionMail");
        action.Context = new Dictionary<string, object>
        {
            ["default_contact_list_ids"] = new[] { Id },
            ["default_mailing_type"] = "mail",
            ["default_model_id"] = Env.Ref<IrModel>("MassMailing.ModelMailingList").Id
        };
        action.Target = "current";
        action.ViewType = "form";
        return action;
    }

    public IrActionsAction ActionViewContacts()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.ActionViewMassMailingContacts");
        action.Domain = new List<object> { new List<object> { "list_ids", "in", new[] { Id } } };
        action.Context = new Dictionary<string, object> { ["default_list_ids"] = new[] { Id } };
        return action;
    }

    public IrActionsAction ActionViewContactsEmail()
    {
        var action = ActionViewContacts();
        action.Context["search_default_filter_valid_email_recipient"] = 1;
        return action;
    }

    public IrActionsAction ActionViewMailings()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.MailingMailingActionMail");
        action.Domain = new List<object> { new List<object> { "contact_list_ids", "in", new[] { Id } } };
        action.Context = new Dictionary<string, object>
        {
            ["default_mailing_type"] = "mail",
            ["default_contact_list_ids"] = new[] { Id }
        };
        return action;
    }

    public IrActionsAction ActionViewContactsOptOut()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.ActionViewMassMailingContacts");
        action.Domain = new List<object> { new List<object> { "list_ids", "in", Id } };
        action.Context = new Dictionary<string, object>
        {
            ["default_list_ids"] = new[] { Id },
            ["create"] = false,
            ["search_default_filter_opt_out"] = 1
        };
        return action;
    }

    public IrActionsAction ActionViewContactsBlacklisted()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.ActionViewMassMailingContacts");
        action.Domain = new List<object> { new List<object> { "list_ids", "in", Id } };
        action.Context = new Dictionary<string, object>
        {
            ["default_list_ids"] = new[] { Id },
            ["create"] = false,
            ["search_default_filter_blacklisted"] = 1
        };
        return action;
    }

    public IrActionsAction ActionViewContactsBouncing()
    {
        var action = Env.Ref<IrActionsAction>("MassMailing.ActionViewMassMailingContacts");
        action.Domain = new List<object> { new List<object> { "list_ids", "in", Id } };
        action.Context = new Dictionary<string, object>
        {
            ["default_list_ids"] = new[] { Id },
            ["create"] = false,
            ["search_default_filter_bounce"] = 1
        };
        return action;
    }

    public void ActionMerge(IEnumerable<MassMailingList> srcLists, bool archive)
    {
        // Implementation of merge action
        // This would need to be implemented using SQL or ORM operations
    }

    public IrActionsActWindowClose CloseDialog()
    {
        return new IrActionsActWindowClose();
    }
}
