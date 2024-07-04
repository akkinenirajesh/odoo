csharp
public partial class EventLeadRule
{
    public override string ToString()
    {
        return Name;
    }

    public List<CrmLead> RunOnRegistrations(List<EventRegistration> registrations)
    {
        // Implementation of _run_on_registrations method
        // This method would need to be adapted to C# and use the Env object for data access and manipulation
        // The logic would remain similar to the Python version, but using C# syntax and constructs
        throw new NotImplementedException();
    }

    public List<EventRegistration> FilterRegistrations(List<EventRegistration> registrations)
    {
        // Implementation of _filter_registrations method
        // This method would need to be adapted to C# and use the Env object for data access
        // The logic would remain similar to the Python version, but using C# syntax and constructs
        throw new NotImplementedException();
    }
}
