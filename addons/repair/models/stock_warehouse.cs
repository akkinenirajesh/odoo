csharp
public partial class StockWarehouse {
    public StockPickingType RepairTypeId { get; set; }
    public bool Active { get; set; }
    public string Code { get; set; }
    public Core.Company CompanyId { get; set; }
    public Stock.Location LotStockId { get; set; }
    public string Name { get; set; }

    public void GetSequenceValues(string name, string code) {
        var values = Env.CallMethod<Dictionary<string, object>>("stock.warehouse", "GetSequenceValues", name, code);
        values.Add("RepairTypeId", new Dictionary<string, object>() {
            { "Name", this.Name + " " + Env.Translate("Sequence repair") },
            { "Prefix", this.Code + "/RO/" },
            { "Padding", 5 },
            { "CompanyId", this.CompanyId.Id }
        });
        return values;
    }

    public (Dictionary<string, object>, int) GetPickingTypeCreateValues(int maxSequence) {
        var (data, nextSequence) = Env.CallMethod<(Dictionary<string, object>, int)>("stock.warehouse", "GetPickingTypeCreateValues", maxSequence);
        var prodLocation = Env.Search<Stock.Location>("usage", "=", "production", "company_id", "=", this.CompanyId.Id).FirstOrDefault();
        var scrapLocation = Env.Search<Stock.Location>("scrap_location", "=", true, "company_id", "in", new List<object>() { this.CompanyId.Id, null }).FirstOrDefault();
        data.Add("RepairTypeId", new Dictionary<string, object>() {
            { "Name", Env.Translate("Repairs") },
            { "Code", "repair_operation" },
            { "DefaultLocationSrcId", this.LotStockId.Id },
            { "DefaultLocationDestId", prodLocation.Id },
            { "DefaultRemoveLocationDestId", scrapLocation.Id },
            { "DefaultRecycleLocationDestId", this.LotStockId.Id },
            { "Sequence", nextSequence + 1 },
            { "SequenceCode", "RO" },
            { "CompanyId", this.CompanyId.Id }
        });
        return (data, maxSequence + 2);
    }

    public Dictionary<string, object> GetPickingTypeUpdateValues() {
        var data = Env.CallMethod<Dictionary<string, object>>("stock.warehouse", "GetPickingTypeUpdateValues");
        data.Add("RepairTypeId", new Dictionary<string, object>() {
            { "Active", this.Active },
            { "Barcode", this.Code.Replace(" ", "").ToUpper() + "RO" }
        });
        return data;
    }
}
