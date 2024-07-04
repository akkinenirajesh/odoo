csharp
public partial class PurchaseOrder {
    public int PickingTypeId { get; set; }
    public int CompanyId { get; set; }

    public void _PopulateFactories() {
        var pickingTypes = Env.GetModel("Stock.PickingType").Search(x => x.Code == "incoming");

        var pickingTypesByCompany = pickingTypes.GroupBy(x => x.CompanyId).ToDictionary(g => g.Key, g => g.ToList());

        var pickingTypesInterCompany = Env.GetModel("Stock.PickingType").Concat(pickingTypesByCompany.GetValueOrDefault(0, new List<Stock.PickingType>()));

        pickingTypesByCompany = pickingTypesByCompany.ToDictionary(x => x.Key, x => Env.GetModel("Stock.PickingType").Concat(x.Value) | pickingTypesInterCompany);

        foreach (var company in pickingTypesByCompany.Keys) {
            if (company != 0) {
                var randomPickingType = pickingTypesByCompany[company].RandomElement();
                this.PickingTypeId = randomPickingType.Id;
            }
        }
    }
}
