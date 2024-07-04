csharp
public partial class Lead
{
    public int ComputeRegistrationCount()
    {
        return Registrations?.Count() ?? 0;
    }

    public void MergeDependences(IEnumerable<Lead> opportunities)
    {
        // Assuming base.MergeDependences is implemented elsewhere in the partial class
        base.MergeDependences(opportunities);

        // Merge registrations as sudo, as crm people may not have access to event rights
        var registrationsToAdd = opportunities.SelectMany(o => o.Registrations).Distinct();
        foreach (var registration in registrationsToAdd)
        {
            if (!Registrations.Contains(registration))
            {
                Registrations.Add(registration);
            }
        }
    }

    public IEnumerable<string> GetMergeFields()
    {
        // Assuming base.GetMergeFields is implemented elsewhere in the partial class
        var baseFields = base.GetMergeFields();
        return baseFields.Concat(new[] { "EventLeadRule", "Event" });
    }
}
