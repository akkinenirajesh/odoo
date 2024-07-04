csharp
public partial class FleetVehicle
{
    public override string ToString()
    {
        return $"{ModelId?.BrandId?.Name ?? ""}/{ModelId?.Name ?? ""}/{LicensePlate ?? "No Plate"}";
    }

    private void _ComputeVehicleName()
    {
        Name = ToString();
    }

    private void _ComputeModelFields()
    {
        if (ModelId != null)
        {
            TrailerHook = ModelId.TrailerHook;
            Color = ModelId.Color;
            Seats = ModelId.Seats;
            ModelYear = ModelId.ModelYear;
            Doors = ModelId.Doors;
            Transmission = ModelId.Transmission;
            FuelType = ModelId.DefaultFuelType;
            Horsepower = ModelId.Horsepower;
            HorsepowerTax = ModelId.HorsepowerTax;
            Power = ModelId.Power;
            Co2 = ModelId.DefaultCo2;
            Co2Standard = ModelId.Co2Standard;
            CategoryId = ModelId.CategoryId;
            ElectricAssistance = ModelId.ElectricAssistance;
        }
    }

    private void _GetOdometer()
    {
        var latestOdometer = Env.Set<FleetVehicleOdometer>()
            .Where(o => o.VehicleId == this)
            .OrderByDescending(o => o.Value)
            .FirstOrDefault();

        Odometer = latestOdometer?.Value ?? 0;
    }

    private void _ComputeCountAll()
    {
        OdometerCount = Env.Set<FleetVehicleOdometer>().Count(o => o.VehicleId == this);
        ServiceCount = Env.Set<FleetVehicleLogServices>().Count(s => s.VehicleId == this && s.Active);
        ContractCount = Env.Set<FleetVehicleLogContract>().Count(c => c.VehicleId == this && c.Active && c.State != ContractState.Closed);
        HistoryCount = Env.Set<FleetVehicleAssignationLog>().Count(h => h.VehicleId == this);
    }

    private void _ComputeContractReminder()
    {
        var params = Env.Set<IrConfigParameter>();
        int delayAlertContract = int.Parse(params.GetParam("hr_fleet.delay_alert_contract", "30"));
        var currentDate = DateTime.Today;

        var latestContract = LogContracts
            .Where(c => c.State != ContractState.Closed)
            .OrderByDescending(c => c.ExpirationDate)
            .FirstOrDefault();

        if (latestContract != null)
        {
            var diffDays = (latestContract.ExpirationDate - currentDate).Days;
            ContractRenewalOverdue = diffDays < 0;
            ContractRenewalDueSoon = !ContractRenewalOverdue && (diffDays < delayAlertContract);
            ContractState = latestContract.State;
        }
        else
        {
            ContractRenewalOverdue = false;
            ContractRenewalDueSoon = false;
            ContractState = null;
        }
    }

    private void _ComputeServiceActivity()
    {
        var activities = LogServices.Select(s => s.ActivityState).Where(s => s != null && s != ActivityState.Planned).ToList();
        ServiceActivity = activities.Any() ? activities.Min() : ServiceActivityState.None;
    }
}
