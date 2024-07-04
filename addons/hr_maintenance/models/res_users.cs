csharp
public partial class Users
{
    public IEnumerable<string> SelfReadableFields()
    {
        var baseFields = base.SelfReadableFields();
        return baseFields.Concat(new[] { "EquipmentCount" });
    }
}

public partial class Employee
{
    public void _ComputeEquipmentCount()
    {
        this.EquipmentCount = this.EquipmentIds.Count();
    }
}
