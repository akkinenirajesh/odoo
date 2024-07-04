csharp
public partial class MrpResCompany {

    public void CreateMissingSubcontractingLocation()
    {
        var companyWithoutSubcontractingLoc = Env.GetModel("Mrp.ResCompany").Search(new [] { new CSharp.Condition("SubcontractingLocationId", "=", null) });
        companyWithoutSubcontractingLoc.ForEach(c => ((MrpResCompany)c).CreateSubcontractingLocation());
    }

    public void CreatePerCompanyLocations()
    {
        // Call super class method
        // ...

        CreateSubcontractingLocation();
    }

    public void CreateSubcontractingLocation()
    {
        var parentLocation = Env.Ref("Stock.StockLocationLocations");

        var subcontractingLocation = Env.GetModel("Stock.Location").Create(new Dictionary<string, object>
        {
            { "Name", "Subcontracting Location" },
            { "Usage", "internal" },
            { "LocationId", parentLocation.Id },
            { "CompanyId", this.Id },
            { "IsSubcontractingLocation", true }
        });

        Env.GetProperty("property_stock_subcontractor", "res.partner").SetDefaultValue(subcontractingLocation, this);
        this.SubcontractingLocationId = subcontractingLocation;
    }
}
