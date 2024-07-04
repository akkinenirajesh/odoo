csharp
public partial class WebsiteSaleLoyalty.LoyaltyCard {
    public WebsiteSaleLoyalty.LoyaltyCard ActionCouponShare() {
        return Env.Create<CouponShare>().CreateShareAction(this);
    }
}
