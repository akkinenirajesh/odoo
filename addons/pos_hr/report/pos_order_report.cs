csharp
public partial class ReportPosOrder
{
    public virtual int EmployeeId { get; set; }

    public virtual string _select()
    {
        return base._select() + ",s.EmployeeId AS EmployeeId";
    }

    public virtual string _groupBy()
    {
        return base._groupBy() + ",s.EmployeeId";
    }
}
