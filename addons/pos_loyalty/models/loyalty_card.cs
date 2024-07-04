C#
public partial class PosLoyalty.LoyaltyCard {

    public void ComputeUseCount()
    {
        var readGroupRes = Env.Ref("Pos.Order.Line")._ReadGroup(
            new List<object>() { new { FieldName = "CouponId", FieldValue = this.Id } }, 
            new List<object>() { "CouponId" },
            new List<object>() { "__count" });
        var countPerCoupon = readGroupRes.Select(x => (x["CouponId"] as int?, (int)x["__count"])).ToDictionary(x => x.Key, x => x.Value);
        this.UseCount += countPerCoupon.GetValueOrDefault(this.Id, 0);
    }

    public bool HasSourceOrder()
    {
        return base.HasSourceOrder() || this.SourcePosOrderId != null;
    }

    public object GetDefaultTemplate()
    {
        if (this.SourcePosOrderId != null)
        {
            return Env.Ref("PosLoyalty.MailCouponTemplate");
        }
        return base.GetDefaultTemplate();
    }

    public object GetMailPartner()
    {
        return base.GetMailPartner() ?? this.SourcePosOrderId.PartnerId;
    }

    public object GetSignature()
    {
        return this.SourcePosOrderId.UserId.Signature ?? base.GetSignature();
    }
}
