csharp
public partial class StockMoveLine {

    public void ComputeExpirationDate() {
        if (this.ExpirationDate == null && this.Lot.ExpirationDate != null) {
            this.ExpirationDate = this.Lot.ExpirationDate;
        }
        else if (this.PickingTypeUseCreateLots) {
            if (this.Product.UseExpirationDate) {
                if (this.ExpirationDate == null) {
                    DateTime fromDate = this.Picking.ScheduledDate ?? DateTime.Now;
                    this.ExpirationDate = fromDate.AddDays(this.Product.ExpirationTime);
                }
            }
            else {
                this.ExpirationDate = null;
            }
        }
    }

    public void OnChangeLotId() {
        if (!this.PickingTypeUseExistingLots || !this.Product.UseExpirationDate) {
            return;
        }
        if (this.Lot != null) {
            this.ExpirationDate = this.Lot.ExpirationDate;
        }
        else {
            this.ExpirationDate = null;
        }
    }

    public void OnChangeProductId() {
        if (this.PickingTypeUseCreateLots) {
            if (this.Product.UseExpirationDate) {
                DateTime fromDate = this.Picking.ScheduledDate ?? DateTime.Now;
                this.ExpirationDate = fromDate.AddDays(this.Product.ExpirationTime);
            }
            else {
                this.ExpirationDate = null;
            }
        }
    }

    public Dictionary<string, object> PrepareNewLotVals() {
        Dictionary<string, object> vals = new Dictionary<string, object>();
        if (this.ExpirationDate != null) {
            vals["ExpirationDate"] = this.ExpirationDate;
        }
        return vals;
    }

    // other methods can be implemented here
}
