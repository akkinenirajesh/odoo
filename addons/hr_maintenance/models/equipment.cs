csharp
public partial class MaintenanceEquipment
{
    public void ComputeOwner()
    {
        OwnerUserId = Env.User;
        if (EquipmentAssignTo == EquipmentAssignTo.Employee)
        {
            OwnerUserId = EmployeeId?.UserId;
        }
        else if (EquipmentAssignTo == EquipmentAssignTo.Department)
        {
            OwnerUserId = DepartmentId?.ManagerId?.UserId;
        }
    }

    public void ComputeEquipmentAssign()
    {
        if (EquipmentAssignTo == EquipmentAssignTo.Employee)
        {
            DepartmentId = null;
        }
        else if (EquipmentAssignTo == EquipmentAssignTo.Department)
        {
            EmployeeId = null;
        }
        AssignDate = DateTime.Today;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        SubscribeAssignees();
    }

    public override void OnWrite()
    {
        base.OnWrite();
        SubscribeAssignees();
    }

    private void SubscribeAssignees()
    {
        var partnerIds = new List<int>();
        if (EmployeeId?.UserId != null)
        {
            partnerIds.Add(EmployeeId.UserId.PartnerId);
        }
        if (DepartmentId?.ManagerId?.UserId != null)
        {
            partnerIds.Add(DepartmentId.ManagerId.UserId.PartnerId);
        }
        if (partnerIds.Any())
        {
            MessageSubscribe(partnerIds);
        }
    }
}
