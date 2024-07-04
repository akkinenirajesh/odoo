csharp
public partial class PurchaseOrder {
    public void ComputeOnTimeRatePerc() {
        if (this.OnTimeRate >= 0) {
            this.OnTimeRatePerc = this.OnTimeRate / 100;
        }
        else {
            this.OnTimeRatePerc = -1;
        }
    }

    public void OnChangeRequisitionId() {
        if (this.RequisitionId != null) {
            this.PickingTypeId = this.RequisitionId.PickingTypeId;
        }
    }
}
