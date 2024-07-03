csharp
public partial class Lead
{
    public bool ComputeShowEnrichButton()
    {
        if (!this.Active || string.IsNullOrEmpty(this.EmailFrom) || 
            this.EmailState == "incorrect" || this.IapEnrichDone || 
            !string.IsNullOrEmpty(this.RevealId) || this.Probability == 100)
        {
            return false;
        }
        return true;
    }

    public void IapEnrich(bool fromCron = false)
    {
        // Implement the enrichment logic here
        // You'll need to adapt the Odoo-specific parts to your C# environment
    }

    public static void IapEnrichLeadsCron(int enrichHoursDelay = 1, int leadsBatchSize = 1000)
    {
        var timeDelta = DateTime.Now.AddHours(-enrichHoursDelay);
        var leads = Env.Search<Lead>(new[]
        {
            ("IapEnrichDone", "=", false),
            ("RevealId", "=", false),
            "|", ("Probability", "<", 100), ("Probability", "=", null),
            ("CreateDate", ">", timeDelta)
        }, limit: leadsBatchSize);

        foreach (var lead in leads)
        {
            lead.IapEnrich(fromCron: true);
        }
    }

    public static void OnCreate(List<Lead> newLeads)
    {
        // Implement the logic for enrichment on create
        // You'll need to adapt this to your C# environment and configuration system
    }

    private void IapEnrichFromResponse(Dictionary<string, object> iapResponse)
    {
        // Implement the enrichment from response logic
        // You'll need to adapt this to your C# environment
    }
}
