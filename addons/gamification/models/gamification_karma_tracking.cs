csharp
public partial class KarmaTracking
{
    public void ComputeGain()
    {
        Gain = NewValue - (OldValue ?? 0);
    }

    public void ComputeOriginRefModelName()
    {
        if (OriginRef == null)
        {
            OriginRefModelName = null;
            return;
        }

        OriginRefModelName = OriginRef.GetType().Name;
    }

    public override KarmaTracking Create(KarmaTracking values)
    {
        if (!values.OldValue.HasValue && values.UserId != null)
        {
            var user = Env.Ref<Core.User>(values.UserId);
            values.OldValue = user.Karma;
        }

        if (values.Gain.HasValue && values.OldValue.HasValue)
        {
            values.NewValue = values.OldValue.Value + values.Gain.Value;
            values.Gain = null;
        }

        return base.Create(values);
    }

    public bool ConsolidateCron()
    {
        var fromDate = DateTime.Today.StartOfMonth().AddMonths(-2);
        return ProcessConsolidate(fromDate);
    }

    public bool ProcessConsolidate(DateTime fromDate, DateTime? endDate = null)
    {
        endDate ??= fromDate.EndOfMonth().EndOfDay();

        // Implementation of consolidation logic
        // This would involve complex SQL operations and data manipulation
        // which would need to be adapted to your specific C# ORM and database access methods

        return true;
    }
}
