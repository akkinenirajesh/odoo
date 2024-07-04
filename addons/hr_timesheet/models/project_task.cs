csharp
public partial class Task
{
    public override string ToString()
    {
        return DisplayName;
    }

    public bool UomInDays()
    {
        return Env.Company.TimesheetEncodeUomId == Env.Ref("uom.product_uom_day");
    }

    public void ComputeEncodeUomInDays()
    {
        EncodeUomInDays = UomInDays();
    }

    public void ComputeAllowTimesheets()
    {
        AllowTimesheets = ProjectId?.AllowTimesheets ?? false;
    }

    public void ComputeAnalyticAccountActive()
    {
        AnalyticAccountActive = GetTaskAnalyticAccountId()?.Active ?? false;
    }

    public void ComputeEffectiveHours()
    {
        EffectiveHours = TimesheetIds.Sum(t => t.UnitAmount);
    }

    public void ComputeProgressHours()
    {
        if (AllocatedHours > 0.0)
        {
            var taskTotalHours = EffectiveHours + SubtaskEffectiveHours;
            Overtime = Math.Max(taskTotalHours - AllocatedHours, 0);
            Progress = Math.Round(taskTotalHours / AllocatedHours, 2);
        }
        else
        {
            Progress = 0.0;
            Overtime = 0;
        }
    }

    public void ComputeRemainingHoursPercentage()
    {
        if (AllocatedHours > 0.0)
        {
            RemainingHoursPercentage = RemainingHours / AllocatedHours;
        }
        else
        {
            RemainingHoursPercentage = 0.0;
        }
    }

    public void ComputeRemainingHours()
    {
        RemainingHours = AllocatedHours - EffectiveHours - SubtaskEffectiveHours;
    }

    public void ComputeTotalHoursSpent()
    {
        TotalHoursSpent = EffectiveHours + SubtaskEffectiveHours;
    }

    public void ComputeSubtaskEffectiveHours()
    {
        SubtaskEffectiveHours = ChildIds.Sum(child => child.EffectiveHours + child.SubtaskEffectiveHours);
    }

    public void ExtractAllocatedHours()
    {
        if (AllowTimesheets)
        {
            var allocatedHoursRegex = new Regex(@"\s(\d+(?:\.\d+)?)[hH]");
            var matches = allocatedHoursRegex.Matches(DisplayName);
            AllocatedHours = matches.Sum(m => float.Parse(m.Groups[1].Value));
            DisplayName = allocatedHoursRegex.Replace(DisplayName, "");
        }
    }

    public float ConvertHoursToDays(float time)
    {
        var uomHour = Env.Ref("uom.product_uom_hour");
        var uomDay = Env.Ref("uom.product_uom_day");
        return (float)Math.Round(uomHour.ComputeQuantity(time, uomDay), 2);
    }
}
