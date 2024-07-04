C#
using System;
using System.Collections.Generic;

public partial class StockRule {
    public virtual int Id { get; set; }
    public virtual string Action { get; set; }
    public virtual Product.Product Product { get; set; }
    public virtual Product.ProductVariant ProductVariant { get; set; }
    public virtual Stock.Warehouse WarehouseId { get; set; }
    public virtual Stock.Location LocationId { get; set; }
    public virtual Stock.Location LocationSrcId { get; set; }
    public virtual Stock.PickingType PickingTypeId { get; set; }
    public virtual Stock.Route RouteId { get; set; }
    public virtual Core.Company Company { get; set; }
    public virtual Stock.Group Group { get; set; }
    public virtual Stock.ProcurementGroup ProcurementGroupId { get; set; }
    public virtual double MinQty { get; set; }
    public virtual double MaxQty { get; set; }
    public virtual bool Signal { get; set; }
    public virtual bool Ignore { get; set; }
    public virtual bool HasError { get; set; }
    public virtual bool Inherit { get; set; }
    public virtual int Delay { get; set; }
    public virtual bool IgnoreVendorLeadTime { get; set; }
    public virtual string ProcurementType { get; set; }
    public virtual DateTime DateExpire { get; set; }
    public virtual DateTime DatePlanned { get; set; }
    public virtual DateTime DateStart { get; set; }
    public virtual DateTime DateEnd { get; set; }
    public virtual int Sequence { get; set; }

    public virtual Dictionary<string, object> GetLeadDays(Product.Product product, Dictionary<string, object> values) {
        var bypassDelayDescription = Env.Context.Get("bypass_delay_description");
        var buyRule = this.Filtered(r => r.Action == "buy");
        var seller = "supplierinfo" in values ? values["supplierinfo"] : product.WithCompany(buyRule.Company).SelectSeller(null);
        if (!buyRule || seller == null) {
            return this.BaseGetLeadDays(product, values);
        }
        seller = (Dictionary<string, object>) seller[0];
        var bom = Env.Get("mrp.bom").Sudo()._BomSubcontractFind(
            product,
            companyId: buyRule.PickingTypeId.Company.Id,
            bomType: "subcontract",
            subcontractor: seller["partner_id"]);
        if (bom == null) {
            return this.BaseGetLeadDays(product, values);
        }

        var delays = this.BaseGetLeadDays(product, values);
        var extraDelays = this.WithContext(new Dictionary<string, object> { { "ignore_vendor_lead_time", true } }).BaseGetLeadDays(product, values);
        if ((int) seller["delay"] >= bom.ProduceDelay + bom.DaysToPrepareMO) {
            delays["total_delay"] += (int) seller["delay"];
            delays["purchase_delay"] += (int) seller["delay"];
            if (!bypassDelayDescription) {
                delays["delay_description"].Add((Env.Translate("Vendor Lead Time"), Env.Translate("+ %d day(s)", (int) seller["delay"])));
            }
        } else {
            var manufactureDelay = bom.ProduceDelay;
            delays["total_delay"] += manufactureDelay;
            // set manufacture_delay to purchase_delay so that PO can be created with correct date
            delays["purchase_delay"] += manufactureDelay;
            if (!bypassDelayDescription) {
                delays["delay_description"].Add((Env.Translate("Manufacturing Lead Time"), Env.Translate("+ %d day(s)", manufactureDelay)));
            }
            var daysToOrder = bom.DaysToPrepareMO;
            delays["total_delay"] += daysToOrder;
            // add dtpmo to purchase_delay so that PO can be created with correct date
            delays["purchase_delay"] += daysToOrder;
            if (!bypassDelayDescription) {
                delays["delay_description"].Add((Env.Translate("Days to Supply Components"), Env.Translate("+ %d day(s)", daysToOrder)));
            }
        }

        foreach (var key in extraDelays.Keys) {
            delays[key] += extraDelays[key];
        }
        return delays;
    }

    private Dictionary<string, object> BaseGetLeadDays(Product.Product product, Dictionary<string, object> values) {
        // implement base logic here
        return new Dictionary<string, object>();
    }

    public virtual List<StockRule> Filtered(Func<StockRule, bool> predicate) {
        // implement filtering logic here
        return new List<StockRule>();
    }

    public virtual StockRule WithContext(Dictionary<string, object> context) {
        // implement context logic here
        return this;
    }

    public virtual Product.Product SelectSeller(int? quantity) {
        // implement select seller logic here
        return null;
    }

    public virtual Mrp.Bom _BomSubcontractFind(Product.Product product, int companyId, string bomType, object subcontractor) {
        // implement bom find logic here
        return null;
    }
}
