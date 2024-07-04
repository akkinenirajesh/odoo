csharp
public partial class Pricelist
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual Core.Currency CurrencyId { get; set; }
    public virtual int Sequence { get; set; }
    public virtual Product.PricelistDiscountPolicy DiscountPolicy { get; set; }
    public virtual Core.Company CompanyId { get; set; }
    public virtual bool Active { get; set; }

    public void Populate(int size)
    {
        // Reflect the settings with data created
        var configSettings = Env.GetModel("Res.ConfigSettings").Create(new Dictionary<string, object>()
        {
            {"GroupProductPricelist", true}, // Activate pricelist
            {"GroupSalePricelist", true}, // Activate advanced pricelist
        });
        configSettings.Execute();

        // Call super method if needed
        // ...
    }
}

public partial class PricelistItem
{
    public virtual int Id { get; set; }
    public virtual Product.Pricelist PricelistId { get; set; }
    public virtual Product.PricelistItemAppliedOn AppliedOn { get; set; }
    public virtual Product.PricelistItemComputePrice ComputePrice { get; set; }
    public virtual double FixedPrice { get; set; }
    public virtual double PercentPrice { get; set; }
    public virtual Product.Product ProductId { get; set; }
    public virtual Product.Template ProductTemplateId { get; set; }
    public virtual Product.Category CategId { get; set; }
    public virtual double MinQuantity { get; set; }
    public virtual Product.PricelistItemBase Base { get; set; }
    public virtual double PriceDiscount { get; set; }
    public virtual DateTime DateStart { get; set; }
    public virtual DateTime DateEnd { get; set; }

}
