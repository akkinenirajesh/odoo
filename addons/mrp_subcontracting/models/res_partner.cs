csharp
public partial class MrpResPartner {
    public void ComputeBomIds() {
        var results = Env.Ref<MrpBom>().ReadGroup(new[] { ("SubcontractorIds.CommercialPartnerId", "in", this.CommercialPartnerId.Ids) }, new[] { "SubcontractorIds" }, new[] { "Id:array_agg" });
        var bomIds = new List<int>();
        foreach (var subcontractor in results) {
            var ids = (List<int>)subcontractor["Id:array_agg"];
            if (this.Id == ((MrpResPartner)subcontractor["SubcontractorIds"]).Id || ((MrpResPartner)subcontractor["SubcontractorIds"]).Id.IsIn(this.ChildIds.Ids)) {
                bomIds.AddRange(ids);
            }
        }
        this.BomIds = bomIds;
    }

    public void ComputeProductionIds() {
        var results = Env.Ref<MrpProduction>().ReadGroup(new[] { ("SubcontractorId.CommercialPartnerId", "in", this.CommercialPartnerId.Ids) }, new[] { "SubcontractorId" }, new[] { "Id:array_agg" });
        var productionIds = new List<int>();
        foreach (var subcontractor in results) {
            var ids = (List<int>)subcontractor["Id:array_agg"];
            if (this.Id == ((MrpResPartner)subcontractor["SubcontractorId"]).Id || ((MrpResPartner)subcontractor["SubcontractorId"]).Id.IsIn(this.ChildIds.Ids)) {
                productionIds.AddRange(ids);
            }
        }
        this.ProductionIds = productionIds;
    }

    public void ComputePickingIds() {
        var results = Env.Ref<StockPicking>().ReadGroup(new[] { ("PartnerId.CommercialPartnerId", "in", this.CommercialPartnerId.Ids) }, new[] { "PartnerId" }, new[] { "Id:array_agg" });
        var pickingIds = new List<int>();
        foreach (var partnerRg in results) {
            var ids = (List<int>)partnerRg["Id:array_agg"];
            if (partnerRg.Id == this.Id || partnerRg.Id.IsIn(this.ChildIds.Ids)) {
                pickingIds.AddRange(ids);
            }
        }
        this.PickingIds = pickingIds;
    }

    public List<int> SearchIsSubcontractor(string operator, bool value) {
        if (!(operator == "=" || operator == "!=" || operator == "<>") || !(value == true || value == false)) {
            throw new Exception("Operation not supported");
        }
        var subcontractorIds = Env.Ref<MrpBom>().Search(new[] { ("Type", "=", "subcontract") }).SubcontractorIds.Ids;
        var searchOperator = value == true ? "in" : "not in";
        return new List<int> { ("Id", searchOperator, subcontractorIds) };
    }

    public void ComputeIsSubcontractor() {
        this.IsSubcontractor = this.UserIds.Any(user => user.IsPortal()) && Env.Ref<MrpBom>().SearchCount(new[] { ("Type", "=", "subcontract"), ("SubcontractorIds", "in", (this | this.CommercialPartnerId).Ids) }, 1) > 0;
    }
}
