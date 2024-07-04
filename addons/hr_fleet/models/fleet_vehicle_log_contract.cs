csharp
public partial class VehicleLogContract
{
    public ActionResult ActionOpenEmployee()
    {
        if (this.PurchaserEmployeeId == null)
        {
            return null;
        }

        return new ActionResult
        {
            Name = "Related Employee",
            Type = ActionType.Window,
            ResModel = "HR.Employee",
            ViewMode = "Form",
            ResId = this.PurchaserEmployeeId.Id
        };
    }
}
