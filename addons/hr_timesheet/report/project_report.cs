csharp
public partial class ReportProjectTaskUser
{
    public override string ToString()
    {
        // You might want to customize this based on your needs
        return $"Task: {this.TaskId}, User: {this.UserId}, Allocated Hours: {this.AllocatedHours}";
    }

    public void CalculateProgress()
    {
        if (this.AllocatedHours > 0)
        {
            this.Progress = (this.EffectiveHours * 100) / this.AllocatedHours;
        }
        else
        {
            this.Progress = 0;
        }
    }

    public void CalculateRemainingHours()
    {
        this.RemainingHours = this.AllocatedHours - this.EffectiveHours - this.SubtaskEffectiveHours;
        if (this.AllocatedHours > 0)
        {
            this.RemainingHoursPercentage = this.RemainingHours / this.AllocatedHours;
        }
        else
        {
            this.RemainingHoursPercentage = 0;
        }
    }

    public float GetTotalHoursSpent()
    {
        return this.EffectiveHours + this.SubtaskEffectiveHours;
    }
}
