C#
public partial class StockMove {
    public virtual ResPartner PartnerId { get; set; }
    public virtual StockLocation LocationId { get; set; }
    public virtual StockLocation LocationDestId { get; set; }
    public virtual List<StockMove> GroupId { get; set; }

    public virtual Dictionary<string, object> PrepareProcurementValues() {
        Dictionary<string, object> vals = base.PrepareProcurementValues();
        ResPartner partner = this.GroupId.FirstOrDefault().PartnerId;
        if (!vals.ContainsKey("PartnerId") && partner != null && this.LocationId.IsSubcontractingLocation) {
            vals["PartnerId"] = partner.Id;
        }
        return vals;
    }

    public virtual bool IsPurchaseReturn() {
        bool res = base.IsPurchaseReturn();
        return res || this.IsDropshippedReturned();
    }

    public virtual bool IsDropshipped() {
        bool res = base.IsDropshipped();
        return res || (
            this.PartnerId.PropertyStockSubcontractor.ParentPath != null &&
            this.PartnerId.PropertyStockSubcontractor.ParentPath.Contains(this.LocationId.ParentPath) &&
            this.LocationDestId.Usage == "customer"
        );
    }

    public virtual bool IsDropshippedReturned() {
        bool res = base.IsDropshippedReturned();
        return res || (
            this.LocationId.Usage == "customer" &&
            this.PartnerId.PropertyStockSubcontractor.ParentPath != null &&
            this.PartnerId.PropertyStockSubcontractor.ParentPath.Contains(this.LocationDestId.ParentPath)
        );
    }
}
