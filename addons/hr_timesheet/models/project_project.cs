csharp
public partial class Project
{
    public bool ComputeEncodeUomInDays()
    {
        return Env.Company.TimesheetEncodeUom == Env.Ref("Uom.ProductUomDay");
    }

    public void ComputeTimesheetEncodeUom()
    {
        this.TimesheetEncodeUom = this.Company?.TimesheetEncodeUom ?? Env.Company.TimesheetEncodeUom;
    }

    public void ComputeAllowTimesheets()
    {
        if (this.AnalyticAccount == null && this._origin != null)
        {
            this.AllowTimesheets = false;
        }
    }

    public void ComputeIsInternalProject()
    {
        this.IsInternalProject = this == this.Company?.InternalProject;
    }

    public void ComputeRemainingHours()
    {
        var timesheetReadGroup = Env.Set<Account.AnalyticLine>()
            .Where(t => t.Project == this)
            .GroupBy(t => t.Project)
            .Select(g => new { Project = g.Key, UnitAmountSum = g.Sum(t => t.UnitAmount) })
            .FirstOrDefault();

        this.EffectiveHours = Math.Round(timesheetReadGroup?.UnitAmountSum ?? 0.0, 2);
        this.RemainingHours = this.AllocatedHours - this.EffectiveHours;
        this.IsProjectOvertime = this.RemainingHours < 0;
    }

    public void ComputeTotalTimesheetTime()
    {
        var timesheetReadGroup = Env.Set<Account.AnalyticLine>()
            .Where(t => t.Project == this)
            .GroupBy(t => new { t.Project, t.ProductUom })
            .Select(g => new { g.Key.Project, g.Key.ProductUom, UnitAmountSum = g.Sum(t => t.UnitAmount) })
            .ToList();

        double totalTime = 0.0;
        foreach (var group in timesheetReadGroup)
        {
            var factor = (group.ProductUom ?? this.TimesheetEncodeUom).FactorInv;
            totalTime += group.UnitAmountSum * (this.EncodeUomInDays ? 1.0 : factor);
        }
        totalTime *= this.TimesheetEncodeUom.Factor;
        this.TotalTimesheetTime = (int)Math.Round(totalTime);
    }

    public string ActionProjectTimesheets()
    {
        // Implementation for opening project timesheets
        // This would typically return an action or navigate to a view
        return "";
    }

    public override string ToString()
    {
        return this.Name;
    }
}
