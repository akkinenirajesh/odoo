C#
public partial class LoyaltyRule {
    public bool Active { get; set; }
    public LoyaltyProgram ProgramId { get; set; }
    public string ProgramType { get; set; }
    public Company CompanyId { get; set; }
    public Currency CurrencyId { get; set; }
    public bool UserHasDebug { get; set; }
    public string ProductDomain { get; set; }
    public List<ProductProduct> ProductIds { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public ProductTag ProductTagId { get; set; }
    public double RewardPointAmount { get; set; }
    public bool RewardPointSplit { get; set; }
    public string RewardPointName { get; set; }
    public string RewardPointMode { get; set; }
    public int MinimumQuantity { get; set; }
    public decimal MinimumAmount { get; set; }
    public string MinimumAmountTaxMode { get; set; }
    public string Mode { get; set; }
    public string Code { get; set; }

    public List<string> GetRewardPointModeSelection() {
        string symbol = Env.Context.Get("currency_symbol", Env.Company.CurrencyId.Symbol);
        return new List<string>() {
            "order",
            $"money_{symbol}",
            "unit"
        };
    }

    public void ComputeMode() {
        if (!string.IsNullOrEmpty(Code)) {
            Mode = "with_code";
        } else {
            Mode = "auto";
        }
    }

    public void ComputeCode() {
        if (Mode == "auto") {
            Code = null;
        }
    }

    public void ComputeUserHasDebug() {
        UserHasDebug = Env.User.IsInGroup("base.group_no_one");
    }
}
