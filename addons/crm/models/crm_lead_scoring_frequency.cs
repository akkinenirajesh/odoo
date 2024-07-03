csharp
public partial class LeadScoringFrequency
{
    public override string ToString()
    {
        return $"{Variable}: {Value}";
    }
}

public partial class LeadScoringFrequencyField
{
    public override string ToString()
    {
        return Name;
    }
}
