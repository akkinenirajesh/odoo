csharp
public partial class FleetVehicleAssignationLog
{
    public void ComputeDriverEmployeeId()
    {
        var employees = Env.Search<Hr.Employee>(e => e.WorkContactId.In(this.DriverId.Ids));
        var employee = employees.FirstOrDefault(e => e.WorkContactId.Id == this.DriverId.Id);
        this.DriverEmployeeId = employee ?? null;
    }

    public void ComputeAttachmentNumber()
    {
        var attachmentData = Env.ReadGroup<Ir.Attachment>(
            a => a.ResModel == "HrFleet.FleetVehicleAssignationLog" && a.ResId == this.Id,
            g => new { ResId = g.ResId, Count = g.Count() }
        );
        
        this.AttachmentNumber = attachmentData.FirstOrDefault()?.Count ?? 0;
    }

    public ActionResult ActionGetAttachmentView()
    {
        var action = Env.Ref<Ir.Actions.ActWindow>("Base.ActionAttachment");
        action.Views = new List<View> { Env.Ref<Ir.UiView>("HrFleet.ViewAttachmentKanbanInheritHr") };
        action.Domain = new Domain(
            ("ResModel", "=", "HrFleet.FleetVehicleAssignationLog"),
            ("ResId", "=", this.Id)
        );
        action.Context = new Dictionary<string, object>
        {
            { "default_res_model", "HrFleet.FleetVehicleAssignationLog" },
            { "default_res_id", this.Id }
        };
        return action;
    }
}
