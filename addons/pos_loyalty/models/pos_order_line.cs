csharp
public partial class PosOrderLine
{
    public bool IsRewardLine { get; set; }

    public Loyalty.Reward RewardId { get; set; }

    public Loyalty.Card CouponId { get; set; }

    public string RewardIdentifierCode { get; set; }

    public double PointsCost { get; set; }

    public bool IsNotSellableLine()
    {
        return Env.Call("Pos.PosOrderLine", "_isNotSellableLine") || this.RewardId != null;
    }

    public List<string> LoadPosDataFields(int configId)
    {
        var params = Env.Call<List<string>>("Pos.PosOrderLine", "_loadPosDataFields", configId);
        params.Add("IsRewardLine");
        params.Add("RewardId");
        params.Add("RewardIdentifierCode");
        params.Add("PointsCost");
        params.Add("CouponId");
        return params;
    }
}
