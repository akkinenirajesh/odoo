csharp
public partial class CalendarEvent
{
    public override Dictionary<string, object> DefaultGet(List<string> fields)
    {
        var defaults = base.DefaultGet(fields);

        if (Env.Context.TryGetValue("DefaultOpportunityId", out var defaultOpportunityId))
        {
            var crmLeadModelId = Env.Ref("Crm.Model_CrmLead").Id;
            Env = Env.WithContext(new Dictionary<string, object>
            {
                { "DefaultResModelId", crmLeadModelId },
                { "DefaultResId", defaultOpportunityId }
            });
        }

        if (!defaults.ContainsKey("OpportunityId") && IsCrmLead(defaults, Env.Context))
        {
            defaults["OpportunityId"] = defaults.TryGetValue("ResId", out var resId)
                ? resId
                : Env.Context.TryGetValue("DefaultResId", out var defaultResId)
                    ? defaultResId
                    : null;
        }

        return defaults;
    }

    public override void ComputeIsHighlighted()
    {
        base.ComputeIsHighlighted();

        if (Env.Context.TryGetValue("ActiveModel", out var activeModel) && activeModel.ToString() == "Crm.Lead")
        {
            if (Env.Context.TryGetValue("ActiveId", out var activeId) && 
                OpportunityId?.Id == (long)activeId)
            {
                IsHighlighted = true;
            }
        }
    }

    public override CalendarEvent Create(Dictionary<string, object> vals)
    {
        var newEvent = base.Create(vals);

        if (newEvent.OpportunityId != null && !newEvent.ActivityIds.Any())
        {
            newEvent.OpportunityId.LogMeeting(newEvent);
        }

        return newEvent;
    }

    private bool IsCrmLead(Dictionary<string, object> defaults, Dictionary<string, object> ctx)
    {
        string resModel = defaults.TryGetValue("ResModel", out var rm) ? rm.ToString() : 
            ctx.TryGetValue("DefaultResModel", out var drm) ? drm.ToString() : null;

        long? resModelId = defaults.TryGetValue("ResModelId", out var rmi) ? (long?)rmi : 
            ctx.TryGetValue("DefaultResModelId", out var drmi) ? (long?)drmi : null;

        return resModel == "Crm.Lead" || 
               (resModelId.HasValue && Env.Get<IrModel>().Browse(resModelId.Value).Model == "Crm.Lead");
    }
}
