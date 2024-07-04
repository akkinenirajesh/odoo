csharp
public partial class VehicleModel
{
    public override string ToString()
    {
        return Brand != null ? $"{Brand.Name}/{Name}" : Name;
    }

    public int ComputeVehicleCount()
    {
        return Env.GetAll<Fleet.Vehicle>().Count(v => v.Model == this);
    }

    public ActionResult ActionModelVehicle()
    {
        return new ActionResult
        {
            Type = ActionType.Window,
            ViewMode = "kanban,tree,form",
            Model = "Fleet.Vehicle",
            Name = "Vehicles",
            Context = new Dictionary<string, object>
            {
                { "search_default_model_id", Id },
                { "default_model_id", Id }
            }
        };
    }
}
