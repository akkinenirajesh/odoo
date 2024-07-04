C#
public partial class PurchaseRequisition {
    public void OnChangeVendor() {
        var requisitions = Env.Search<PurchaseRequisition>(new[] {
            new SearchCondition("Vendor", "=", this.Vendor.Id),
            new SearchCondition("State", "=", PurchaseRequisitionState.Confirmed),
            new SearchCondition("RequisitionType", "=", RequisitionType.BlanketOrder),
            new SearchCondition("Company", "=", this.Company.Id)
        });
        if (requisitions.Any()) {
            var title = $"Warning for {this.Vendor.Name}";
            var message = "There is already an open blanket order for this supplier. We suggest you complete this open blanket order, instead of creating a new one.";
            var warning = new { Title = title, Message = message };
            Env.Notify(warning);
        }
    }

    public void ComputeCurrencyId() {
        if (!this.Vendor.HasValue || !this.Vendor.Value.PropertyPurchaseCurrencyId.HasValue) {
            this.Currency = this.Company.CurrencyId;
        } else {
            this.Currency = this.Vendor.Value.PropertyPurchaseCurrencyId;
        }
    }

    public void ComputeOrdersNumber() {
        this.OrderCount = this.PurchaseOrders.Count();
    }

    public void CheckDates() {
        if (this.DateEnd.HasValue && this.DateStart.HasValue && this.DateEnd.Value < this.DateStart.Value) {
            throw new Exception($"End date cannot be earlier than start date. Please check dates for agreements: {String.Join(", ", this.Name)}");
        }
    }

    public void Create() {
        if (this.RequisitionType == RequisitionType.BlanketOrder) {
            this.Name = Env.GetNextSequence("Purchase.PurchaseRequisition.BlanketOrder", this.Company.Id);
        } else {
            this.Name = Env.GetNextSequence("Purchase.PurchaseRequisition.PurchaseTemplate", this.Company.Id);
        }
    }

    public void Write() {
        if (this.State != PurchaseRequisitionState.Draft) {
            throw new Exception("You cannot change the Agreement Type or Company of a not draft purchase agreement.");
        }
        if (this.RequisitionType == RequisitionType.PurchaseTemplate) {
            this.DateStart = null;
            this.DateEnd = null;
        }
        var code = this.RequisitionType == RequisitionType.BlanketOrder ? "Purchase.PurchaseRequisition.BlanketOrder" : "Purchase.PurchaseRequisition.PurchaseTemplate";
        this.Name = Env.GetNextSequence(code, this.Company.Id);
    }

    public void Unlink() {
        this.LineIds.Unlink();
    }

    public void ActionCancel() {
        foreach (var line in this.LineIds) {
            line.SupplierInfoIds.Unlink();
        }
        this.PurchaseOrders.ButtonCancel();
        foreach (var purchaseOrder in this.PurchaseOrders) {
            purchaseOrder.MessagePost(new { Body = "Cancelled by the agreement associated to this quotation." });
        }
        this.State = PurchaseRequisitionState.Cancel;
    }

    public void ActionConfirm() {
        if (!this.LineIds.Any()) {
            throw new Exception($"You cannot confirm agreement '{this.Name}' because it does not contain any product lines.");
        }
        if (this.RequisitionType == RequisitionType.BlanketOrder) {
            foreach (var line in this.LineIds) {
                if (line.PriceUnit <= 0.0) {
                    throw new Exception("You cannot confirm a blanket order with lines missing a price.");
                }
                if (line.ProductQty <= 0.0) {
                    throw new Exception("You cannot confirm a blanket order with lines missing a quantity.");
                }
                line.CreateSupplierInfo();
            }
        }
        this.State = PurchaseRequisitionState.Confirmed;
    }

    public void ActionDraft() {
        this.State = PurchaseRequisitionState.Draft;
    }

    public void ActionDone() {
        if (this.PurchaseOrders.Any(po => po.State == PurchaseOrderState.Draft || po.State == PurchaseOrderState.Sent || po.State == PurchaseOrderState.ToApprove)) {
            throw new Exception("To close this purchase requisition, cancel related Requests for Quotation.\n\nImagine the mess if someone confirms these duplicates: double the order, double the trouble :)");
        }
        foreach (var line in this.LineIds) {
            line.SupplierInfoIds.Unlink();
        }
        this.State = PurchaseRequisitionState.Done;
    }

    public void UnlinkIfDraftOrCancel() {
        if (this.State != PurchaseRequisitionState.Draft && this.State != PurchaseRequisitionState.Cancel) {
            throw new Exception("You can only delete draft or cancelled requisitions.");
        }
    }
}
