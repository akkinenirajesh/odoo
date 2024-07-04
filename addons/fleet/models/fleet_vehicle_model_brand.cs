csharp
public partial class VehicleModelBrand
{
    public void ComputeModelCount()
    {
        var modelData = Env.Get<Fleet.VehicleModel>().ReadGroup(
            new[] { ("Brand", "in", new[] { this.Id }) },
            new[] { "Brand" },
            new[] { "__count" }
        );

        var modelsBrand = modelData.ToDictionary(
            item => item.Brand.Id,
            item => item.__count
        );

        this.ModelCount = modelsBrand.GetValueOrDefault(this.Id, 0);
    }

    public ActionResult ActionBrandModel()
    {
        return new ActionResult
        {
            Type = ActionType.Window,
            ViewMode = "tree,form",
            ResModel = "Fleet.VehicleModel",
            Name = "Models",
            Context = new Dictionary<string, object>
            {
                { "search_default_brand_id", this.Id },
                { "default_brand_id", this.Id }
            }
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
