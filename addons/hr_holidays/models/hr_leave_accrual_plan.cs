csharp
public partial class LeaveAccrualPlan
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeShowTransitionMode()
    {
        ShowTransitionMode = Levels.Count() > 1;
    }

    public void ComputeLevelCount()
    {
        LevelCount = Levels.Count();
    }

    public void ComputeEmployeeCount()
    {
        EmployeesCount = Allocations.Select(a => a.Employee).Distinct().Count();
    }

    public void ComputeCompanyId()
    {
        if (TimeOffType != null)
        {
            Company = TimeOffType.Company;
        }
        else
        {
            Company = Env.Company;
        }
    }

    public void ComputeIsBasedOnWorkedTime()
    {
        if (AccruedGainTime == AccruedGainTime.Start)
        {
            IsBasedOnWorkedTime = false;
        }
    }

    public void ComputeAddedValueType()
    {
        if (Levels.Any())
        {
            AddedValueType = Levels.First().AddedValueType;
        }
    }

    public void ComputeCarryoverDayDisplay()
    {
        var daysSelect = GetSelectionDays();
        CarryoverDayDisplay = daysSelect[Math.Min(CarryoverDay - 1, 28)];
    }

    public void InverseCarryoverDayDisplay()
    {
        if (CarryoverDayDisplay == "last")
        {
            CarryoverDay = 31;
        }
        else
        {
            CarryoverDay = int.Parse(CarryoverDayDisplay);
        }
    }

    public ActionResult ActionOpenAccrualPlanEmployees()
    {
        return new ActionResult
        {
            Name = "Accrual Plan's Employees",
            Type = ActionType.Window,
            ViewMode = "kanban,tree,form",
            Model = "Hr.Employee",
            Domain = new Domain(nameof(Employee.Id), "in", Allocations.Select(a => a.Employee.Id).ToList())
        };
    }

    public LeaveAccrualPlan Copy()
    {
        var copy = (LeaveAccrualPlan)MemberwiseClone();
        copy.Name = $"{Name} (copy)";
        return copy;
    }
}
