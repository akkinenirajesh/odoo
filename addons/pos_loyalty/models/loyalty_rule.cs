C#
public partial class LoyaltyRule {

    public virtual LoyaltyProgram ProgramId { get; set; }
    public virtual ICollection<Product.Product> ValidProductIds { get; set; }
    public virtual bool AnyProduct { get; set; }
    public virtual string PromoBarcode { get; set; }
    public virtual double RewardPointAmount { get; set; }
    public virtual double RewardPointSplit { get; set; }
    public virtual RewardPointMode RewardPointMode { get; set; }
    public virtual int MinimumQty { get; set; }
    public virtual double MinimumAmount { get; set; }
    public virtual TaxMode MinimumAmountTaxMode { get; set; }
    public virtual LoyaltyRuleMode Mode { get; set; }
    public virtual string Code { get; set; }
    public virtual ResCurrency.Currency CurrencyId { get; set; }

    public virtual void ComputeValidProductIds()
    {
        // Implement logic to compute valid product IDs based on the rule
        // Use Env to access other models and data
        // Example:
        // var products = Env.Get<Product.Product>().Search(new[] {
        //     new Domain("available_in_pos", "=", true),
        //     new Domain("id", "in", ValidProductIds.Select(p => p.Id).ToList()),
        //     // Add other conditions based on rule criteria
        // });
        // ValidProductIds = products;
        // AnyProduct = products.Count() == 0;
    }

    public virtual void ComputePromoBarcode()
    {
        // Implement logic to compute PromoBarcode based on code
        // Use Env to access other models and data
        // Example:
        // PromoBarcode = Env.Get<LoyaltyCard>().GenerateCode();
    }

    // Implement other methods and logic as required
}
