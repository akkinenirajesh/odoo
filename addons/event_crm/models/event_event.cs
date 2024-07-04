csharp
public partial class EventEvent
{
    public void ComputeHasLeadRequest()
    {
        var leadRequestsData = Env.Get<EventLeadRequest>().ReadGroup(
            new[] { ("EventId", "in", new[] { this.Id }) },
            new[] { "EventId" },
            new[] { "__count" }
        );
        var mappedData = leadRequestsData.ToDictionary(x => x.EventId, x => x.__count);
        this.HasLeadRequest = mappedData.GetValueOrDefault(this.Id, 0) != 0;
    }

    public void ComputeLeadCount()
    {
        var leadData = Env.Get<CrmLead>().ReadGroup(
            new[] { ("EventId", "in", new[] { this.Id }) },
            new[] { "EventId" },
            new[] { "__count" }
        );
        var mappedData = leadData.ToDictionary(x => x.EventId, x => x.__count);
        this.LeadCount = mappedData.GetValueOrDefault(this.Id, 0);
    }

    public ActionResult ActionGenerateLeads()
    {
        if (!Env.User.HasGroup("event.group_event_manager"))
        {
            throw new UserError("Only Event Managers are allowed to re-generate all leads.");
        }

        var registrationsCount = Env.Get<EventRegistration>().Search(new[]
        {
            ("EventId", "=", this.Id),
            ("State", "not in", new[] { "draft", "cancel" })
        }).Count();

        string notification;
        if (registrationsCount <= Env.Get<EventLeadRequest>().RegistrationsBatchSize)
        {
            var leads = Env.Get<EventRegistration>().Search(new[]
            {
                ("EventId", "=", this.Id),
                ("State", "not in", new[] { "draft", "cancel" })
            }).ApplyLeadGenerationRules();

            notification = leads.Any()
                ? $"Yee-ha, {leads.Count} Leads have been created!"
                : "Aww! No Leads created, check your Lead Generation Rules and try again.";
        }
        else
        {
            Env.Get<EventLeadRequest>().Sudo().Create(new { EventId = this.Id });
            Env.Ref("event_crm.ir_cron_generate_leads").Trigger();
            notification = "Got it! We've noted your request. Your leads will be created soon!";
        }

        return new ActionResult
        {
            Type = "ir.actions.client",
            Tag = "display_notification",
            Params = new
            {
                Type = "info",
                Sticky = false,
                Message = notification,
                Next = new { Type = "ir.actions.act_window_close" }
            }
        };
    }
}
