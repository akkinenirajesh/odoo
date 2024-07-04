csharp
public partial class SaleTimesheet.ReportProjectTaskUser 
{
    public virtual float RemainingHoursSo { get; set; }

    public virtual string Select()
    {
        return base.Select() + @",
            sol.RemainingHours as RemainingHoursSo
        ";
    }

    public virtual string GroupBy()
    {
        return base.GroupBy() + @",
            sol.RemainingHours
        ";
    }

    public virtual string From()
    {
        return base.From() + @"
            LEFT JOIN Sale.SaleOrderLine sol ON t.Id = sol.TaskId
        ";
    }
}
