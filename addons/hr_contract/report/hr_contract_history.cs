csharp
public partial class ContractHistory
{
    public override string ToString()
    {
        return $"{EmployeeId.Name}'s Contracts History";
    }

    public void ComputeContractCount()
    {
        ContractCount = ContractIds.Count();
    }

    public void ComputeUnderContractState()
    {
        UnderContractState = IsUnderContract ? UnderContractState.Done : UnderContractState.Blocked;
    }

    public ActionResult HrContractViewFormNewAction()
    {
        var action = Env.Actions.ForXmlId("HrContract.ActionHrContract");
        action.Context["default_employee_id"] = EmployeeId.Id;
        action.ViewMode = "form";
        action.ViewId = Env.Ref("HrContract.HrContractViewForm").Id;
        action.Views = new List<(int, string)> { (Env.Ref("HrContract.HrContractViewForm").Id, "form") };
        return action;
    }
}
