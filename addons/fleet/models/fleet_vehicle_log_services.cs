csharp
public partial class VehicleLogService
{
    public override string ToString()
    {
        return ServiceType?.ToString() ?? string.Empty;
    }

    public decimal GetOdometer()
    {
        return Odometer?.Value ?? 0;
    }

    public void SetOdometer(decimal value)
    {
        if (value == 0)
        {
            throw new UserException("Emptying the odometer value of a vehicle is not allowed.");
        }

        var odometer = Env.Create<Fleet.VehicleOdometer>(new
        {
            Value = value,
            Date = Date ?? DateTime.Now,
            Vehicle = Vehicle
        });

        Odometer = odometer;
    }

    protected override void OnCreating()
    {
        base.OnCreating();

        if (OdometerValue == 0)
        {
            OdometerValue = null;
        }
    }

    protected override void OnComputing()
    {
        base.OnComputing();

        if (Vehicle != null)
        {
            Purchaser = Vehicle.Driver;
        }
    }
}
