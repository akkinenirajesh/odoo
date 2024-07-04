csharp
public partial class PosHr.PosConfig
{
    public void OnChangeBasicEmployeeIds()
    {
        foreach (var employee in this.BasicEmployeeIds)
        {
            if (this.AdvancedEmployeeIds.Contains(employee))
            {
                this.AdvancedEmployeeIds.Remove(employee);
            }
        }
    }

    public void OnChangeAdvancedEmployeeIds()
    {
        foreach (var employee in this.AdvancedEmployeeIds)
        {
            if (this.BasicEmployeeIds.Contains(employee))
            {
                this.BasicEmployeeIds.Remove(employee);
            }
        }
    }
}
