csharp
public partial class VehicleOdometer
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeVehicleLogName()
    {
        string name = Vehicle?.Name ?? "";
        if (string.IsNullOrEmpty(name))
        {
            name = Date.ToString();
        }
        else if (Date != null)
        {
            name += " / " + Date.ToString();
        }
        Name = name;
    }

    public void OnChangeVehicle()
    {
        if (Vehicle != null)
        {
            Unit = Vehicle.OdometerUnit;
        }
    }
}
