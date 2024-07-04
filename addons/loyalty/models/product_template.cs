csharp
public partial class Loyalty.ProductTemplate
{
    public void UnlinkExceptLoyaltyProducts()
    {
        var productData = new List<Loyalty.ProductVariant>()
        {
            Env.Ref<Loyalty.ProductVariant>("loyalty.gift_card_product_50"),
            Env.Ref<Loyalty.ProductVariant>("loyalty.ewallet_product_50")
        };

        var products = this.Filtered(p => productData.Contains(p.ProductVariantId));

        if (products.Any())
        {
            throw new UserError(
                $"You cannot delete {this.WithContext(new Dictionary<string, object>() { { "display_default_code", false } }).DisplayName} as it is used in 'Coupons & Loyalty'. Please archive it instead."
            );
        }
    }
}
