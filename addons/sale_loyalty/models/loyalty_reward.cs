csharp
public partial class SaleLoyaltyReward
{
    public void Unlink()
    {
        if (this.Env.Context.Get("active_ids").Count == 1 && this.Env.Model("Sale.OrderLine").SearchCount(new[] { new SearchTerm("RewardId", "in", new[] { this.Id }) }, 1) > 0)
        {
            this.ActionArchive();
        }
        else
        {
            base.Unlink();
        }
    }

    private List<Dictionary<string, object>> GetDiscountProductValues()
    {
        var res = base.GetDiscountProductValues();
        foreach (var vals in res)
        {
            vals.AddOrUpdate("TaxesId", null);
            vals.AddOrUpdate("SupplierTaxesId", null);
            vals.AddOrUpdate("InvoicePolicy", "order");
        }
        return res;
    }
}
