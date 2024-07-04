csharp
public partial class SaleLoyaltyCard {
    public SaleLoyaltyCard() {
    }

    public virtual int GetDefaultTemplate() {
        var defaultTemplate = base.GetDefaultTemplate();
        if (defaultTemplate == 0) {
            defaultTemplate = Env.Ref("loyalty.mail_template_loyalty_card");
        }
        return defaultTemplate;
    }

    public virtual int GetMailPartner() {
        return base.GetMailPartner() ?? this.OrderId.PartnerId;
    }

    public virtual string GetSignature() {
        return this.OrderId.UserId.Signature ?? base.GetSignature();
    }

    public virtual void ComputeUseCount() {
        base.ComputeUseCount();
        var readGroupRes = Env.Model("Sale.SaleOrderLine").ReadGroup(new List<object>() {new Dictionary<string, object>() {{"CouponId", this.Id}}}, new List<string>() {"CouponId"}, new List<string>() {"__count"});
        var countPerCoupon = readGroupRes.Select((x, i) => new { CouponId = x[0], Count = x[1] }).ToDictionary(x => x.CouponId, x => x.Count);
        this.UseCount += countPerCoupon.ContainsKey(this.Id) ? countPerCoupon[this.Id] : 0;
    }

    public virtual bool HasSourceOrder() {
        return base.HasSourceOrder() || this.OrderId != null;
    }
}
