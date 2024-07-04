csharp
public partial class MrpSubcontractingDropshipping.StockWarehouse 
{
    public MrpSubcontractingDropshipping.StockWarehouse Create(Dictionary<string, object> values)
    {
        var res = Env.Call("super", "Create", values);
        if (values.ContainsKey("SubcontractingDropshippingToResupply") && values["SubcontractingDropshippingToResupply"] is bool && (bool)values["SubcontractingDropshippingToResupply"])
        {
            this.UpdateGlobalRouteDropshipSubcontractor();
        }
        return res;
    }

    public MrpSubcontractingDropshipping.StockWarehouse Write(Dictionary<string, object> values)
    {
        var res = Env.Call("super", "Write", values);
        if (values.ContainsKey("SubcontractingDropshippingToResupply") || values.ContainsKey("Active"))
        {
            if (values.ContainsKey("SubcontractingDropshippingToResupply"))
            {
                this._UpdateDropshipSubcontractRules();
            }
            this.UpdateGlobalRouteDropshipSubcontractor();
        }
        return res;
    }

    public void _UpdateDropshipSubcontractRules()
    {
        var subcontractingLocations = this._GetSubcontractingLocations();
        var routeId = this._FindOrCreateGlobalRoute("MrpSubcontractingDropshipping.RouteSubcontractingDropshipping", "Dropship Subcontractor on Order");
        var warehousesDropship = this.Where(w => w.SubcontractingDropshippingToResupply && w.Active).ToList();
        if (warehousesDropship.Any())
        {
            Env.Model("Stock.StockRule").WithContext(new Dictionary<string, object> { { "active_test", false } }).Search(new List<object> {
                new List<object> { "route_id", "=", routeId.Id },
                new List<object> { "action", "=", "pull" },
                new List<object> { "warehouse_id", "in", warehousesDropship.Select(w => w.Id).ToList() },
                new List<object> { "location_src_id", "in", subcontractingLocations.Select(l => l.Id).ToList() }
            }).ActionUnarchive();
        }
        var warehousesNoDropship = this.Except(warehousesDropship).ToList();
        if (warehousesNoDropship.Any())
        {
            Env.Model("Stock.StockRule").Search(new List<object> {
                new List<object> { "route_id", "=", routeId.Id },
                new List<object> { "action", "=", "pull" },
                new List<object> { "warehouse_id", "in", warehousesNoDropship.Select(w => w.Id).ToList() },
                new List<object> { "location_src_id", "in", subcontractingLocations.Select(l => l.Id).ToList() }
            }).ActionArchive();
        }
    }

    public void UpdateGlobalRouteDropshipSubcontractor()
    {
        var routeId = this._FindOrCreateGlobalRoute("MrpSubcontractingDropshipping.RouteSubcontractingDropshipping", "Dropship Subcontractor on Order");
        var allRules = routeId.Sudo().RuleIds.Where(r => r.Active).ToList();
        foreach (var company in this.CompanyId)
        {
            var companyRules = allRules.Where(r => r.CompanyId == company).ToList();
            company.DropshipSubcontractorPickTypeId.Active = companyRules.Where(r => r.Action == "pull").Any();
        }
        routeId.Active = allRules.Where(r => r.Action == "pull").Any();
    }

    public Dictionary<string, object> _GenerateGlobalRouteRulesValues()
    {
        var rules = Env.Call("super", "_GenerateGlobalRouteRulesValues");
        var subcontractLocationId = this._GetSubcontractingLocation();
        var productionLocationId = this._GetProductionLocation();
        rules.Add("SubcontractingDropshippingPullId", new Dictionary<string, object>
        {
            { "depends", new List<string> { "SubcontractingDropshippingToResupply" } },
            { "create_values", new Dictionary<string, object>
            {
                { "procure_method", "make_to_order" },
                { "company_id", this.CompanyId.Id },
                { "action", "pull" },
                { "auto", "manual" },
                { "route_id", this._FindOrCreateGlobalRoute("MrpSubcontractingDropshipping.RouteSubcontractingDropshipping", "Dropship Subcontractor on Order").Id },
                { "name", this._FormatRulename(subcontractLocationId, productionLocationId, false) },
                { "location_dest_id", productionLocationId.Id },
                { "location_src_id", subcontractLocationId.Id },
                { "picking_type_id", this.SubcontractingTypeId.Id }
            } },
            { "update_values", new Dictionary<string, object>
            {
                { "active", this.SubcontractingDropshippingToResupply }
            } }
        });
        return rules;
    }

    // All other methods should be implemented here.
    private List<Stock.StockLocation> _GetSubcontractingLocations() { throw new NotImplementedException(); }
    private Stock.StockLocation _GetSubcontractingLocation() { throw new NotImplementedException(); }
    private Stock.StockLocation _GetProductionLocation() { throw new NotImplementedException(); }
    private string _FormatRulename(Stock.StockLocation subcontractLocationId, Stock.StockLocation productionLocationId, bool flag) { throw new NotImplementedException(); }
    private Stock.StockRule _FindOrCreateGlobalRoute(string routeId, string name) { throw new NotImplementedException(); }
}
