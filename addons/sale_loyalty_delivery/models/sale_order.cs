C#
public partial class SaleOrder
{
    public virtual float ComputeAmountTotalWithoutDelivery()
    {
        var amountTotalWithoutDelivery = this.Env.Call<SaleOrder>("_compute_amount_total_without_delivery", this);
        var deliveryLines = this.OrderLines.Where(l => l.Coupon != null && l.Coupon.ProgramType.In("ewallet", "gift_card"));
        amountTotalWithoutDelivery -= deliveryLines.Sum(l => l.PriceUnit);
        return amountTotalWithoutDelivery;
    }

    public virtual object _get_no_effect_on_threshold_lines()
    {
        return this.Env.Call<SaleOrder>("_get_no_effect_on_threshold_lines", this);
    }

    public virtual object _get_not_rewarded_order_lines()
    {
        var orderLines = this.Env.Call<SaleOrder>("_get_not_rewarded_order_lines", this);
        return orderLines.Where(line => !line.IsDelivery);
    }

    public virtual object _get_reward_values_free_shipping(Sale.Reward reward, Sale.CouponProgram coupon)
    {
        var deliveryLine = this.OrderLines.Where(l => l.IsDelivery).FirstOrDefault();
        var taxes = deliveryLine.Product.Taxes.Where(tax => tax.Company == this.Company.Id).ToList();
        taxes = this.Company.FiscalPosition.Taxes;
        var maxDiscount = reward.DiscountMaxAmount ?? float.MaxValue;
        return new List<object>()
        {
            new
            {
                Name = $"Free Shipping - {reward.Description}",
                Reward = reward.Id,
                Coupon = coupon.Id,
                PointsCost = reward.ClearWallet ? this.Env.Call<SaleOrder>("_get_real_points_for_coupon", coupon) : reward.RequiredPoints,
                Product = reward.DiscountLineProduct.Id,
                PriceUnit = -Math.Min(maxDiscount, deliveryLine.PriceUnit ?? 0),
                ProductUomQty = 1,
                ProductUom = reward.DiscountLineProduct.Uom.Id,
                OrderId = this.Id,
                IsRewardLine = true,
                Sequence = Math.Max(this.OrderLines.Where(x => !x.IsRewardLine).Max(x => x.Sequence), 0) + 1,
                Taxes = new List<object>()
                {
                    new
                    {
                        Command = "CLEAR",
                        Id = 0,
                        Field = 0
                    }
                }.Concat(taxes.Select(tax => new
                {
                    Command = "LINK",
                    Id = tax.Id,
                    Field = false
                })).ToList()
            }
        };
    }

    public virtual object _get_reward_line_values(Sale.Reward reward, Sale.CouponProgram coupon)
    {
        if (reward.RewardType == "shipping")
        {
            var lang = this.Env.Context.Get("lang");
            reward = reward.WithContext(lang);
            return this._get_reward_values_free_shipping(reward, coupon);
        }
        return this.Env.Call<SaleOrder>("_get_reward_line_values", this, reward, coupon);
    }

    public virtual object _get_claimable_rewards(List<Sale.CouponProgram> forcedCoupons = null)
    {
        var res = this.Env.Call<SaleOrder>("_get_claimable_rewards", this, forcedCoupons);
        if (this.OrderLines.Any(reward => reward.Reward.RewardType == "shipping"))
        {
            var filteredRes = new Dictionary<Sale.CouponProgram, List<Sale.Reward>>();
            foreach (var coupon in res.Keys)
            {
                var filteredRewards = res[coupon].Where(r => r.RewardType != "shipping").ToList();
                if (filteredRewards.Count > 0)
                {
                    filteredRes.Add(coupon, filteredRewards);
                }
            }
            res = filteredRes;
        }
        return res;
    }

    public virtual object _get_real_points_for_coupon(Sale.CouponProgram coupon)
    {
        return this.Env.Call<SaleOrder>("_get_real_points_for_coupon", this, coupon);
    }
}
