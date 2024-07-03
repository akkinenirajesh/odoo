csharp
public partial class BaseAutomation
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeUrl()
    {
        if (Trigger == TriggerType.OnWebhook)
        {
            Url = $"{GetBaseUrl()}/web/hook/{WebhookUuid}";
        }
        else
        {
            Url = string.Empty;
        }
    }

    public void ComputeTriggerFieldRefModelName()
    {
        if ((Trigger == TriggerType.OnStageSet || Trigger == TriggerType.OnTagSet) && TriggerFieldRef != null)
        {
            var relation = TriggerFieldIds.FirstOrDefault()?.Relation;
            TriggerFieldRefModelName = !string.IsNullOrEmpty(relation) ? relation : null;
        }
        else
        {
            TriggerFieldRefModelName = null;
        }
    }

    public void ComputeLeastDelayMsg()
    {
        int interval = GetCronInterval();
        LeastDelayMsg = $"Note that this automation rule can be triggered up to {interval} minutes after its schedule.";
    }

    private int GetCronInterval(IEnumerable<BaseAutomation> automations = null)
    {
        // Implementation of _get_cron_interval logic
        // ...
    }

    private string GetBaseUrl()
    {
        // Implementation to get the base URL
        // ...
    }

    // Other methods would be implemented similarly, adapting Odoo's Python logic to C#
    // ...
}
