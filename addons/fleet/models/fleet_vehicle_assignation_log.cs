csharp
public partial class VehicleAssignationLog
{
    public override string ToString()
    {
        return ComputeDisplayName();
    }

    private string ComputeDisplayName()
    {
        var vehicle = Env.Get<Fleet.Vehicle>(VehicleId);
        var driver = Env.Get<Core.Partner>(DriverId);
        return $"{vehicle.Name} - {driver.Name}";
    }
}
