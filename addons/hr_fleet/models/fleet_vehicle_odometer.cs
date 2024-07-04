csharp
public partial class FleetVehicleOdometer
{
    public override string ToString()
    {
        // Implement a meaningful string representation
        return $"Odometer reading for {Vehicle?.Name ?? "Unknown Vehicle"}";
    }

    // If you need to add any custom logic or methods, you can do so here
}
