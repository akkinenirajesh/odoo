csharp
public partial class LeaveReport
{
    public ActionResult ActionOpenRecord()
    {
        var action = new ActionResult
        {
            Type = ActionType.ActWindow,
            ViewMode = "form"
        };

        if (Leave != null)
        {
            action.ResId = Leave.Id;
            action.ResModel = "HR.Leave";
        }
        else if (Allocation != null)
        {
            action.ResId = Allocation.Id;
            action.ResModel = "HR.LeaveAllocation";
        }

        return action;
    }

    public override string ToString()
    {
        return $"{Employee?.Name} - {Name}";
    }
}
