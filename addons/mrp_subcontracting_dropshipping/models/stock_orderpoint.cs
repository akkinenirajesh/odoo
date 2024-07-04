csharp
public partial class StockWarehouseOrderpoint {
    public StockWarehouseOrderpoint(Env env) {
        this.Env = env;
    }

    public Env Env { get; set; }

    public StockLocation LocationId { get; set; }
    public ResPartner PartnerId { get; set; }
    public ProductProduct ProductId { get; set; }
    public string Name { get; set; }
    public float ProductQty { get; set; }
    public float LeadTime { get; set; }
    public float MinQty { get; set; }
    public float MaxQty { get; set; }
    public float OrderpointQty { get; set; }
    public ResPartner VendorId { get; set; }
    public StockRoute RouteId { get; set; }
    public StockProcurementRule ProcurementRuleId { get; set; }
    public bool Active { get; set; }
    public bool IsSubcontractor { get; set; }

    public virtual Dictionary<string, object> PrepareProcurementValues(DateTime? date = null, string group = null) {
        var vals = this.Env.Call("stock.warehouse.orderpoint", "_prepare_procurement_values", this, date, group);
        if (!vals.ContainsKey("partner_id") && this.LocationId.IsSubcontractingLocation) {
            var subcontractors = this.LocationId.SubcontractorIds;
            if (subcontractors.Count == 1) {
                vals["partner_id"] = subcontractors[0].Id;
            } else {
                vals["partner_id"] = false;
            }
        }
        return vals;
    }
}
