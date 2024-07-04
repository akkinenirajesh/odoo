csharp
public partial class DepartureReason
{
    public override string ToString()
    {
        return Name;
    }

    public Dictionary<string, int> GetDefaultDepartureReasons()
    {
        return new Dictionary<string, int>
        {
            { "fired", 342 },
            { "resigned", 343 },
            { "retired", 340 }
        };
    }

    public void OnDelete()
    {
        var masterDepartureCodes = GetDefaultDepartureReasons().Values;
        if (masterDepartureCodes.Contains(this.ReasonCode))
        {
            throw new UserException("Default departure reasons cannot be deleted.");
        }
    }
}
