csharp
public partial class MessageSubtype
{
    public MessageSubtype GetDepartmentSubtype()
    {
        return Env.MessageSubtypes.FirstOrDefault(st =>
            st.ResModel == "Hr.Department" &&
            st.ParentId == this);
    }

    public MessageSubtype UpdateDepartmentSubtype()
    {
        var departmentSubtype = GetDepartmentSubtype();
        if (departmentSubtype != null)
        {
            departmentSubtype.Name = this.Name;
            departmentSubtype.Default = this.Default;
        }
        else
        {
            departmentSubtype = Env.MessageSubtypes.Create(new MessageSubtype
            {
                Name = this.Name,
                ResModel = "Hr.Department",
                Default = this.Default,
                ParentId = this,
                RelationField = "DepartmentId"
            });
        }
        return departmentSubtype;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (ResModel == "Hr.Leave" || ResModel == "Hr.LeaveAllocation")
        {
            UpdateDepartmentSubtype();
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (ResModel == "Hr.Leave" || ResModel == "Hr.LeaveAllocation")
        {
            UpdateDepartmentSubtype();
        }
    }
}
