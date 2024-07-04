csharp
public partial class FleetVehicleLogServices
{
    public void ComputePurchaserId()
    {
        var internals = Env.Query<FleetVehicleLogServices>()
            .Where(r => r.PurchaserEmployeeId != null)
            .ToList();

        // Assuming there's a base implementation for non-internal records
        base.ComputePurchaserId();

        foreach (var service in internals)
        {
            service.PurchaserId = service.PurchaserEmployeeId?.WorkContactId;
        }
    }

    public void ComputePurchaserEmployeeId()
    {
        PurchaserEmployeeId = VehicleId?.DriverEmployeeId;
    }
}
