csharp
public partial class SalePurchase.ProductTemplate {
    public void CheckServiceToPurchase() {
        if (this.ServiceToPurchase) {
            if (this.Type != "service") {
                throw new ValidationError("Product that is not a service can not create RFQ.");
            }
            CheckVendorForServiceToPurchase(this.SellerIds);
        }
    }

    public void CheckVendorForServiceToPurchase(List<SalePurchase.ProductSupplierInfo> sellers) {
        if (sellers == null || sellers.Count == 0) {
            throw new ValidationError("Please define the vendor from whom you would like to purchase this service automatically.");
        }
    }

    public void OnChangeServiceToPurchase() {
        if (this.Type != "service" || this.ExpensePolicy != "no") {
            this.ServiceToPurchase = false;
        }
    }
}
