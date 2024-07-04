csharp
public partial class SaleOrderLine {
    public bool IsRewardLine { get; set; }
    public Loyalty.Reward RewardId { get; set; }
    public Loyalty.Card CouponId { get; set; }
    public string RewardIdentifierCode { get; set; }
    public double PointsCost { get; set; }

    public void ComputeName() {
        if (RewardId != null) {
            return;
        }
        // Call super class method to compute name
        // ...
    }

    public void ComputeIsRewardLine() {
        IsRewardLine = RewardId != null;
    }

    public void ComputeTaxId() {
        if (IsRewardLine) {
            return;
        }
        // Call super class method to compute tax id
        // ...
        // Additional code for reward lines
        // ...
    }

    public double GetDisplayPrice() {
        if (IsRewardLine && RewardId.RewardType != "product") {
            return PriceUnit;
        }
        // Call super class method to get display price
        // ...
    }

    public bool CanBeInvoicedAlone() {
        return base.CanBeInvoicedAlone() && !IsRewardLine;
    }

    public bool IsNotSellableLine() {
        return IsRewardLine || base.IsNotSellableLine();
    }

    public void ResetLoyalty(bool complete) {
        PointsCost = 0;
        PriceUnit = 0;

        if (complete) {
            CouponId = null;
            RewardId = null;
        }
    }

    public void Create(SaleOrderLine[] valsList) {
        // Call super class method to create
        // ...
        // Update coupon points
        // ...
    }

    public void Write(SaleOrderLine[] vals) {
        // Call super class method to write
        // ...
        // Update coupon points
        // ...
    }

    public void Unlink() {
        // Get related reward lines
        // ...
        // Remove coupon from order
        // ...
        // Give back points
        // ...
        // Call super class method to unlink
        // ...
        // Unlink coupons
        // ...
    }
}
