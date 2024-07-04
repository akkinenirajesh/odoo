csharp
public partial class MrpWorkorder
{
    public void ComputeDuration()
    {
        // Call super._compute_duration()
        CreateOrUpdateAnalyticEntry();
    }

    public void SetDuration()
    {
        // Call super._set_duration()
        CreateOrUpdateAnalyticEntry();
    }

    public void ActionCancel()
    {
        Env.Delete(this.MoAnalyticAccountLineIds.Union(this.WcAnalyticAccountLineIds));
        // Call super.action_cancel()
    }

    private void CreateOrUpdateAnalyticEntry()
    {
        if (!this.Id.HasValue || !this.ProductionId.HasValue || !this.WorkcenterId.HasValue || this.WcAnalyticAccountLineIds.Any() || this.MoAnalyticAccountLineIds.Any())
        {
            var hours = this.Duration / 60.0;
            var value = -hours * this.WorkcenterId.CostsHour;

            var moAnalyticLineVals = Env.CallMethod("account.analytic.account", "_perform_analytic_distribution", 
                this.ProductionId.AnalyticDistribution, value, hours, this.MoAnalyticAccountLineIds, this);
            if (moAnalyticLineVals != null)
            {
                this.MoAnalyticAccountLineIds.AddRange(Env.Create("account.analytic.line", moAnalyticLineVals));
            }

            var wcAnalyticLineVals = Env.CallMethod("account.analytic.account", "_perform_analytic_distribution", 
                this.WorkcenterId.AnalyticDistribution, value, hours, this.WcAnalyticAccountLineIds, this);
            if (wcAnalyticLineVals != null)
            {
                this.WcAnalyticAccountLineIds.AddRange(Env.Create("account.analytic.line", wcAnalyticLineVals));
            }
        }
    }

    public void Unlink()
    {
        Env.Delete(this.MoAnalyticAccountLineIds.Union(this.WcAnalyticAccountLineIds));
        // Call super.unlink()
    }
}
