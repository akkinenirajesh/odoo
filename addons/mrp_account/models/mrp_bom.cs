C#
public partial class MrpBom {

    public void ComputeAnalyticDistribution() {
        this.AnalyticDistribution = Env.Json.Parse(this.AnalyticDistributionText ?? "{}");
    }

    public void InverseAnalyticDistribution() {
        this.AnalyticDistributionText = Env.Json.Stringify(this.AnalyticDistribution);
    }

    public void ComputeAnalyticAccountIds() {
        this.AnalyticAccountIds = Env.Database.Browse<Account.AnalyticAccount>(
            Env.Linq.Select(this.AnalyticDistribution, ids => ids.Split(',').Select(int.Parse)).Distinct()
        ).ToList();
    }

    public void OnChangeProductId() {
        if (this.Product != null) {
            this.AnalyticDistribution = Env.Model<Account.AnalyticDistributionModel>()._GetDistribution(new Dictionary<string, object> {
                { "ProductId", this.Product.Id },
                { "ProductCategId", this.Product.CategId.Id },
                { "CompanyId", this.Company.Id }
            });
        }
    }

    public void CheckAnalytic() {
        Env.Context.Set("validateAnalytic", true);
        this._ValidateDistribution(new Dictionary<string, object> {
            { "Product", this.Product.Id },
            { "CompanyId", this.Company.Id }
        });
    }

    private void _ValidateDistribution(Dictionary<string, object> parameters) {
        // Implementation for _ValidateDistribution
    }
}
