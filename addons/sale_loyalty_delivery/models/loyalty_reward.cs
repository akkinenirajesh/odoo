C#
public partial class SaleLoyaltyDelivery.LoyaltyReward {
    public void ComputeDescription() {
        if (this.RewardType == "shipping") {
            this.Description = "Free shipping";
            if (this.DiscountMaxAmount > 0) {
                string formattedAmount = string.Format("{0} {1}", this.CurrencyId.Symbol, this.DiscountMaxAmount);
                this.Description += string.Format(" (Max {0})", formattedAmount);
            }
        }
        else {
            // Call the base method to compute description for other reward types
            // ...
        }
    }
}
