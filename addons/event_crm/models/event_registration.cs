csharp
public partial class EventRegistration
{
    public int ComputeLeadCount()
    {
        return LeadIds.Count();
    }

    public override EventRegistration Create()
    {
        var registration = base.Create();

        if (!Env.Context.ContainsKey("event_lead_rule_skip"))
        {
            registration.ApplyLeadGenerationRules();
        }

        return registration;
    }

    public override void Write(Dictionary<string, object> vals)
    {
        var toUpdate = false;
        var eventLeadRuleSkip = Env.Context.ContainsKey("event_lead_rule_skip");

        if (!eventLeadRuleSkip && LeadCount > 0)
        {
            toUpdate = true;
            var leadTrackedVals = GetLeadTrackedValues();
        }

        base.Write(vals);

        if (!eventLeadRuleSkip && toUpdate)
        {
            Env.FlushAll();
            UpdateLeads(vals, leadTrackedVals);
        }

        if (!eventLeadRuleSkip)
        {
            if (vals.ContainsKey("State") && (string)vals["State"] == "open")
            {
                Env.GetModel<EventLeadRule>().Search(new[] { ("LeadCreationTrigger", "=", "confirm") }).RunOnRegistrations(new[] { this });
            }
            else if (vals.ContainsKey("State") && (string)vals["State"] == "done")
            {
                Env.GetModel<EventLeadRule>().Search(new[] { ("LeadCreationTrigger", "=", "done") }).RunOnRegistrations(new[] { this });
            }
        }
    }

    public IEnumerable<CrmLead> ApplyLeadGenerationRules()
    {
        var leads = new List<CrmLead>();
        var openRegistrations = this.Where(reg => reg.State == "open");
        var doneRegistrations = this.Where(reg => reg.State == "done");

        leads.AddRange(Env.GetModel<EventLeadRule>().Search(new[] { ("LeadCreationTrigger", "=", "create") }).RunOnRegistrations(new[] { this }));

        if (openRegistrations.Any())
        {
            leads.AddRange(Env.GetModel<EventLeadRule>().Search(new[] { ("LeadCreationTrigger", "=", "confirm") }).RunOnRegistrations(openRegistrations));
        }

        if (doneRegistrations.Any())
        {
            leads.AddRange(Env.GetModel<EventLeadRule>().Search(new[] { ("LeadCreationTrigger", "=", "done") }).RunOnRegistrations(doneRegistrations));
        }

        return leads;
    }

    // ... Other methods like UpdateLeads, GetLeadValues, GetLeadContactValues, etc. would be implemented here ...

    public override string ToString()
    {
        return Name;
    }
}
