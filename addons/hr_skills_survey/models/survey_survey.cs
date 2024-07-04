csharp
public partial class SurveySurvey
{
    public override string ToString()
    {
        // Assuming there's a Title field in the SurveySurvey model
        return this.Title ?? string.Empty;
    }
}
