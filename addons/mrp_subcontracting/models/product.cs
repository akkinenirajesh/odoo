C#
public partial class Mrp.SupplierInfo {
  public void ComputeIsSubcontractor() {
    var boms = Env.Model("Mrp.Bom").Search(new { ProductId = this.ProductId });
    boms.Add(Env.Model("Mrp.Bom").Search(new { ProductTmplId = this.ProductTmplId, ProductId = null }));
    boms = boms.Where(b => b.ProductId == null || b.ProductId == this.ProductId).ToList();
    this.IsSubcontractor = boms.Any(b => b.SubcontractorIds.Contains(this.PartnerId));
  }
}

public partial class Mrp.ProductProduct {
  public List<Mrp.SupplierInfo> PrepareSellers(Dictionary<string, object> params = null) {
    if (params != null && params.ContainsKey("SubcontractorIds")) {
      return Env.Model<Mrp.SupplierInfo>().Search(new { PartnerId = params["SubcontractorIds"] });
    }

    return Env.Model<Mrp.SupplierInfo>().Search(new { });
  }
}
