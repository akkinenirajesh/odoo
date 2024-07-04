C#
public partial class PurchaseRequisitionStock.StockRule {
  public PurchaseRequisitionStock.StockRule PreparePurchaseOrder(int companyID, List<object> origins, Dictionary<string, object> values) {
    var res = Env.Call("super", "_prepare_purchase_order", companyID, origins, values);
    values = values.First();
    res["PartnerRef"] = values["Supplier"].Get("PurchaseRequisitionId").Get("Name");
    res["RequisitionId"] = values["Supplier"].Get("PurchaseRequisitionId").Get("Id");
    if (values["Supplier"].Get("PurchaseRequisitionId").Get("CurrencyId") != null) {
      res["CurrencyId"] = values["Supplier"].Get("PurchaseRequisitionId").Get("CurrencyId").Get("Id");
    }
    return res;
  }
  public List<object> MakePOGetDomain(int companyID, Dictionary<string, object> values, object partner) {
    var domain = Env.Call("super", "_make_po_get_domain", companyID, values, partner);
    if (values.ContainsKey("Supplier") && values["Supplier"].Get("PurchaseRequisitionId") != null) {
      domain.Add(new List<object> {
        "RequisitionId",
        "=",
        values["Supplier"].Get("PurchaseRequisitionId").Get("Id"),
      });
    }
    return domain;
  }
}
public partial class PurchaseRequisitionStock.StockMove {
  public List<object> GetUpstreamDocumentsAndResponsibles(List<object> visited) {
    var requisitionLinesSudo = this.Get("RequisitionLineIds").sudo();
    if (requisitionLinesSudo.Count > 0) {
      return requisitionLinesSudo.Where(x => x.Get("RequisitionId").Get("State") != "done" && x.Get("RequisitionId").Get("State") != "cancel").Select(x => new List<object> {
        x.Get("RequisitionId"),
        x.Get("RequisitionId").Get("UserId"),
        visited,
      }).ToList();
    } else {
      return Env.Call("super", "_get_upstream_documents_and_responsibles", visited);
    }
  }
}
