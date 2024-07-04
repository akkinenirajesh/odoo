csharp
public partial class PurchaseResPartner {

    public void ComputePurchaseOrderCount() {
        // retrieve all children partners and prefetch 'parent_id' on them
        var allPartners = Env.Model("Purchase.ResPartner").SearchFetch(
            new string[] { "ParentId" },
            new[] { ("Id", "ChildOf", new[] { this.Id }) }
        );
        var purchaseOrderGroups = Env.Model("Purchase.Order").ReadGroup(
            new[] { ("PartnerId", "In", allPartners.Ids) },
            new[] { "PartnerId" },
            new[] { "__count" }
        );
        var selfIds = new HashSet<long>(new[] { this.Id });

        this.PurchaseOrderCount = 0;
        foreach (var group in purchaseOrderGroups) {
            var partnerId = (long)group["PartnerId"];
            var count = (int)group["__count"];
            while (partnerId != 0) {
                if (selfIds.Contains(partnerId)) {
                    this.PurchaseOrderCount += count;
                }
                partnerId = Env.Model("Purchase.ResPartner").Browse(partnerId).ParentId;
            }
        }
    }

    public void ComputeSupplierInvoiceCount() {
        // retrieve all children partners and prefetch 'parent_id' on them
        var allPartners = Env.Model("Purchase.ResPartner").SearchFetch(
            new string[] { "ParentId" },
            new[] { ("Id", "ChildOf", new[] { this.Id }) }
        );
        var supplierInvoiceGroups = Env.Model("Account.Move").ReadGroup(
            new[] { ("PartnerId", "In", allPartners.Ids), ("MoveType", "In", new[] { "in_invoice", "in_refund" }) },
            new[] { "PartnerId" },
            new[] { "__count" }
        );
        var selfIds = new HashSet<long>(new[] { this.Id });

        this.SupplierInvoiceCount = 0;
        foreach (var group in supplierInvoiceGroups) {
            var partnerId = (long)group["PartnerId"];
            var count = (int)group["__count"];
            while (partnerId != 0) {
                if (selfIds.Contains(partnerId)) {
                    this.SupplierInvoiceCount += count;
                }
                partnerId = Env.Model("Purchase.ResPartner").Browse(partnerId).ParentId;
            }
        }
    }
}
