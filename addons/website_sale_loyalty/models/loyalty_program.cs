csharp
public partial class WebsiteSaleLoyalty.LoyaltyProgram
{
    public WebsiteSaleLoyalty.LoyaltyProgram ActionProgramShare()
    {
        this.EnsureOne();
        return Env.Create<Coupon.Share>().CreateShareAction(program: this);
    }
}
