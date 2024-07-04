csharp
public partial class Loyalty.ProductProduct
{
    public void Write(Dictionary<string, object> vals)
    {
        if (!vals.ContainsKey("Active") || (bool)vals["Active"] == true && this.Active)
        {
            // Prevent archiving products used for giving rewards
            var rewards = Env.Search<Loyalty.Reward>(new Dictionary<string, object>() { { "DiscountLineProductId", this.Id }, { "Active", true } }, 1);
            if (rewards.Count > 0)
            {
                throw new Exception("This product may not be archived. It is being used for an active promotion program.");
            }
        }
        Env.Write(this, vals);
    }

    public void OnDelete()
    {
        var productData = new List<Loyalty.ProductProduct>
        {
            Env.Ref<Loyalty.ProductProduct>("loyalty.gift_card_product_50"),
            Env.Ref<Loyalty.ProductProduct>("loyalty.ewallet_product_50")
        };
        if (productData.Contains(this))
        {
            throw new Exception("You cannot delete " + this.DisplayName + " as it is used in 'Coupons & Loyalty'. Please archive it instead.");
        }
    }
}
