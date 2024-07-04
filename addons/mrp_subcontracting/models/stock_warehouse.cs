C#
public partial class MrpSubcontracting.StockWarehouse {
    public MrpSubcontracting.StockWarehouse Create(Dictionary<string, object> vals) {
        var res = Env.Model("stock.warehouse").Create(vals);
        res.UpdateSubcontractingLocationsRules();
        if (vals.ContainsKey("SubcontractingToResupply") && (bool)vals["SubcontractingToResupply"]) {
            res.UpdateGlobalRouteResupplySubcontractor();
        }
        return res;
    }

    public void Write(Dictionary<string, object> vals) {
        Env.Model("stock.warehouse").Write(this.Id, vals);
        if (vals.ContainsKey("SubcontractingToResupply") || vals.ContainsKey("Active")) {
            if (vals.ContainsKey("SubcontractingToResupply")) {
                this.UpdateResupplyRules();
            }
            this.UpdateGlobalRouteResupplySubcontractor();
        }
    }

    public Dictionary<int, Dictionary<string, List<object>>> GetRulesDict() {
        var result = (Dictionary<int, Dictionary<string, List<object>>>)Env.Model("stock.warehouse").Call("GetRulesDict", this);
        var subcontractLocationId = this.GetSubcontractingLocation().Id;
        result[this.Id].Add("Subcontract", new List<object> {
            new {
                LotStockId = this.LotStockId,
                SubcontractLocationId = subcontractLocationId,
                SubcontractingResupplyTypeId = this.SubcontractingResupplyTypeId,
                Action = "Pull"
            }
        });
        return result;
    }

    public void UpdateGlobalRouteResupplySubcontractor() {
        var routeId = this.FindOrCreateGlobalRoute("mrp_subcontracting.route_resupply_subcontractor_mto", "Resupply Subcontractor on Order");
        if (!Env.Model("stock.rule").Search(new Dictionary<string, object> { { "RouteId", routeId }, { "Active", true } }).Any()) {
            routeId.Active = false;
        } else {
            routeId.Active = true;
        }
    }

    public Dictionary<string, object> GetRoutesValues() {
        var routes = (Dictionary<string, object>)Env.Model("stock.warehouse").Call("GetRoutesValues", this);
        routes.Add("SubcontractingRouteId", new Dictionary<string, object> {
            { "RoutingKey", "Subcontract" },
            { "Depends", new List<string> { "SubcontractingToResupply" } },
            { "RouteCreateValues", new Dictionary<string, object> {
                { "ProductCategSelectable", false },
                { "WarehouseSelectable", true },
                { "ProductSelectable", false },
                { "CompanyId", this.CompanyId },
                { "Sequence", 10 },
                { "Name", this.FormatRoutename("Resupply Subcontractor") }
            } },
            { "RouteUpdateValues", new Dictionary<string, object> {
                { "Active", this.SubcontractingToResupply }
            } },
            { "RulesValues", new Dictionary<string, object> {
                { "Active", this.SubcontractingToResupply }
            } }
        });
        return routes;
    }

    public Dictionary<string, object> GenerateGlobalRouteRulesValues() {
        var rules = (Dictionary<string, object>)Env.Model("stock.warehouse").Call("GenerateGlobalRouteRulesValues", this);
        var subcontractLocationId = this.GetSubcontractingLocation().Id;
        var productionLocationId = this.GetProductionLocation().Id;
        rules.Add("SubcontractingMtoPullId", new Dictionary<string, object> {
            { "Depends", new List<string> { "SubcontractingToResupply" } },
            { "CreateValues", new Dictionary<string, object> {
                { "ProcureMethod", "MakeToOrder" },
                { "CompanyId", this.CompanyId },
                { "Action", "Pull" },
                { "Auto", "Manual" },
                { "RouteId", this.FindOrCreateGlobalRoute("stock.route_warehouse0_mto", "Replenish on Order (MTO)").Id },
                { "Name", this.FormatRulename(this.LotStockId, subcontractLocationId, "MTO") },
                { "LocationDestId", subcontractLocationId },
                { "LocationSrcId", this.LotStockId },
                { "PickingTypeId", this.SubcontractingResupplyTypeId }
            } },
            { "UpdateValues", new Dictionary<string, object> {
                { "Active", this.SubcontractingToResupply }
            } }
        });
        rules.Add("SubcontractingPullId", new Dictionary<string, object> {
            { "Depends", new List<string> { "SubcontractingToResupply" } },
            { "CreateValues", new Dictionary<string, object> {
                { "ProcureMethod", "MakeToOrder" },
                { "CompanyId", this.CompanyId },
                { "Action", "Pull" },
                { "Auto", "Manual" },
                { "RouteId", this.FindOrCreateGlobalRoute("mrp_subcontracting.route_resupply_subcontractor_mto", "Resupply Subcontractor on Order").Id },
                { "Name", this.FormatRulename(subcontractLocationId, productionLocationId, false) },
                { "LocationDestId", productionLocationId },
                { "LocationSrcId", subcontractLocationId },
                { "PickingTypeId", this.SubcontractingResupplyTypeId }
            } },
            { "UpdateValues", new Dictionary<string, object> {
                { "Active", this.SubcontractingToResupply }
            } }
        });
        return rules;
    }

    public Tuple<Dictionary<string, object>, int> GetPickingTypeCreateValues(int maxSequence) {
        var data = (Dictionary<string, object>)Env.Model("stock.warehouse").Call("GetPickingTypeCreateValues", this, maxSequence);
        data.Add("SubcontractingTypeId", new Dictionary<string, object> {
            { "Name", "Subcontracting" },
            { "Code", "mrp_operation" },
            { "UseCreateComponentsLots", true },
            { "Sequence", maxSequence + 2 },
            { "SequenceCode", "SBC" },
            { "CompanyId", this.CompanyId }
        });
        data.Add("SubcontractingResupplyTypeId", new Dictionary<string, object> {
            { "Name", "Resupply Subcontractor" },
            { "Code", "internal" },
            { "UseCreateLots", false },
            { "UseExistingLots", true },
            { "DefaultLocationDestId", this.GetSubcontractingLocation().Id },
            { "Sequence", maxSequence + 3 },
            { "SequenceCode", "RES" },
            { "PrintLabel", true },
            { "CompanyId", this.CompanyId }
        });
        return Tuple.Create(data, maxSequence + 4);
    }

    public Dictionary<string, object> GetSequenceValues(string name = null, string code = null) {
        var values = (Dictionary<string, object>)Env.Model("stock.warehouse").Call("GetSequenceValues", this, name, code);
        var count = Env.Model("ir.sequence").SearchCount(new Dictionary<string, object> { { "Prefix", $"like {this.Code}/SBC%/" } });
        values.Add("SubcontractingTypeId", new Dictionary<string, object> {
            { "Name", $"{this.Name} Sequence subcontracting" },
            { "Prefix", $"{this.Code}{(count > 0 ? $"/SBC{count}/" : "/SBC/")}" },
            { "Padding", 5 },
            { "CompanyId", this.CompanyId }
        });
        values.Add("SubcontractingResupplyTypeId", new Dictionary<string, object> {
            { "Name", $"{this.Name} Sequence Resupply Subcontractor" },
            { "Prefix", $"{this.Code}{(count > 0 ? $"/RES{count}/" : "/RES/")}" },
            { "Padding", 5 },
            { "CompanyId", this.CompanyId }
        });
        return values;
    }

    public Dictionary<string, object> GetPickingTypeUpdateValues() {
        var data = (Dictionary<string, object>)Env.Model("stock.warehouse").Call("GetPickingTypeUpdateValues", this);
        var subcontractLocationId = this.GetSubcontractingLocation().Id;
        var productionLocationId = this.GetProductionLocation().Id;
        data.Add("SubcontractingTypeId", new Dictionary<string, object> {
            { "Active", false },
            { "DefaultLocationSrcId", subcontractLocationId },
            { "DefaultLocationDestId", productionLocationId }
        });
        data.Add("SubcontractingResupplyTypeId", new Dictionary<string, object> {
            { "DefaultLocationSrcId", this.LotStockId },
            { "DefaultLocationDestId", subcontractLocationId },
            { "Barcode", $"{this.Code.Replace(" ", "").ToUpper()}RESUP" },
            { "Active", this.SubcontractingToResupply && this.Active }
        });
        return data;
    }

    public MrpSubcontracting.StockLocation GetSubcontractingLocation() {
        return (MrpSubcontracting.StockLocation)Env.Model("stock.location").Search(new Dictionary<string, object> {
            { "CompanyId", this.CompanyId },
            { "IsSubcontractingLocation", true }
        }).First();
    }

    public List<MrpSubcontracting.StockLocation> GetSubcontractingLocations() {
        return Env.Model("stock.location").Search(new Dictionary<string, object> {
            { "CompanyId", this.CompanyId },
            { "IsSubcontractingLocation", true }
        });
    }

    public void UpdateSubcontractingLocationsRules() {
        this.GetSubcontractingLocations().ActivateSubcontractingLocationRules();
    }

    public void UpdateResupplyRules() {
        var subcontractingLocations = this.GetSubcontractingLocations();
        var warehousesToResupply = Env.Model("stock.warehouse").Search(new Dictionary<string, object> {
            { "SubcontractingToResupply", true },
            { "Active", true }
        });
        if (warehousesToResupply.Any()) {
            Env.Model("stock.rule").Search(new Dictionary<string, object> {
                { "PickingTypeId", warehousesToResupply.Select(w => w.SubcontractingResupplyTypeId).ToList() },
                { "LocationSrcId", subcontractingLocations.Select(l => l.Id).ToList() },
                { "LocationDestId", subcontractingLocations.Select(l => l.Id).ToList() }
            }).Unarchive();
        }
        var warehousesNotToResupply = Env.Model("stock.warehouse").Search(new Dictionary<string, object> {
            { "SubcontractingToResupply", false },
            { "Active", true }
        });
        if (warehousesNotToResupply.Any()) {
            Env.Model("stock.rule").Search(new Dictionary<string, object> {
                { "PickingTypeId", warehousesNotToResupply.Select(w => w.SubcontractingResupplyTypeId).ToList() },
                { "LocationSrcId", subcontractingLocations.Select(l => l.Id).ToList() },
                { "LocationDestId", subcontractingLocations.Select(l => l.Id).ToList() }
            }).Archive();
        }
    }
}
