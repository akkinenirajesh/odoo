csharp
public partial class EventStage
{
    public override string ToString()
    {
        return Name;
    }

    public string GetDefaultLegendBlocked()
    {
        return Env.T("Blocked");
    }

    public string GetDefaultLegendDone()
    {
        return Env.T("Ready for Next Stage");
    }

    public string GetDefaultLegendNormal()
    {
        return Env.T("In Progress");
    }
}
