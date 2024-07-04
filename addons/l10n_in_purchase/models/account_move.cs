csharp
public partial class AccountMove
{
    [OnChange("PurchaseVendorBillId", "PurchaseId")]
    public void OnChangePurchaseAutoComplete()
    {
        var purchaseOrderId = PurchaseVendorBillId?.PurchaseOrder ?? PurchaseId;
        if (purchaseOrderId != null && purchaseOrderId.CountryCode == "IN")
        {
            L10nInGstTreatment = purchaseOrderId.L10nInGstTreatment;
        }
        base.OnChangePurchaseAutoComplete();
    }
}
