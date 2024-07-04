csharp
public partial class AccrualPlanLevel
{
    public override string ToString()
    {
        // Implement string representation logic here
        return base.ToString();
    }

    public void ComputeFrequencyHourlySource()
    {
        if (AccruedGainTime == "start")
        {
            FrequencyHourlySource = FrequencyHourlySource.Calendar;
        }
    }
}
