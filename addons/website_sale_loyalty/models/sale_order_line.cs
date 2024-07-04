csharp
public partial class WebsiteSaleLoyalty.SaleOrderLine 
{
    public bool ComputeShowInCart()
    {
        return Env.Get("RewardId").Get("RewardType") != "discount" && base.ComputeShowInCart();
    }

    public void Unlink()
    {
        if (Env.Context.Get("website_sale_loyalty_delete") == true)
        {
            var disabledRewardsPerOrder = new Dictionary<WebsiteSale.SaleOrder, List<WebsiteSaleLoyalty.LoyaltyReward>>();
            foreach (var line in this.Env.Get("SaleOrderLine").Search([["Id", "in", this.Env.Get("Id").ToList()]]))
            {
                if (line.Get("RewardId") != null)
                {
                    if (!disabledRewardsPerOrder.ContainsKey(line.Get("OrderId")))
                    {
                        disabledRewardsPerOrder.Add(line.Get("OrderId"), new List<WebsiteSaleLoyalty.LoyaltyReward>());
                    }
                    disabledRewardsPerOrder[line.Get("OrderId")].Add(line.Get("RewardId"));
                }
            }
            foreach (var pair in disabledRewardsPerOrder)
            {
                pair.Key.Set("DisabledAutoRewards", pair.Value);
            }
        }
        base.Unlink();
    }
}
