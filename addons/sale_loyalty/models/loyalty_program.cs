csharp
public partial class SaleLoyaltyProgram {
    public int OrderCount { get; set; }
    public bool SaleOk { get; set; }

    public void ComputeOrderCount() {
        // An order should count only once PER program but may appear in multiple programs
        var readGroupRes = Env.GetModel("Sale.Order.Line").ReadGroup(
            new[] { new Domain("RewardId", "in", this.RewardIds.Ids) },
            new[] { "OrderId" },
            new[] { new GroupBy("RewardId", "array_agg") }
        );

        var programRewardIds = this.RewardIds.Ids;
        this.OrderCount = readGroupRes.Sum(x => {
            var rewardIds = (List<int>)x[1];
            return rewardIds.Any(id_ => programRewardIds.Contains(id_));
        });
    }

    public void ComputeTotalOrderCount() {
        Env.GetModel("Sale.LoyaltyProgram").ComputeTotalOrderCount(this);
        this.TotalOrderCount += this.OrderCount;
    }
}
