csharp
public partial class Product.ResCompany
{
    public void ActivateOrCreatePricelists()
    {
        if (Env.Context.ContainsKey("disable_company_pricelist_creation"))
            return;

        if (Env.User.HasGroup("product.group_product_pricelist"))
        {
            var companies = this;
            var ProductPricelist = Env.Model<Product.ProductPricelist>().Sudo();
            var defaultPricelistsSudo = ProductPricelist.WithContext(new { active_test = false }).Search(x => x.ItemIds.IsNullOrEmpty() && companies.Contains(x.CompanyId));
            defaultPricelistsSudo.ActionUnarchive();
            var companiesWithoutPricelist = companies.Where(x => !defaultPricelistsSudo.Select(y => y.CompanyId).Contains(x.Id));
            ProductPricelist.Create(companiesWithoutPricelist.Select(x => x.GetDefaultPricelistVals()).ToList());
        }
    }

    public ResCompany GetDefaultPricelistVals()
    {
        return new ResCompany
        {
            Name = "Default",
            Currency = this.Currency,
            CompanyId = this.Id,
            Sequence = 10
        };
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (!vals.ContainsKey("CurrencyId"))
        {
            base.Write(vals);
            return;
        }

        var enabledPricelists = Env.User.HasGroup("product.group_product_pricelist");
        base.Write(vals);
        if (!enabledPricelists && Env.User.HasGroup("product.group_product_pricelist"))
        {
            this.ActivateOrCreatePricelists();
        }
    }
}
