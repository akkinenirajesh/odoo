csharp
public partial class Website.Product
{
    public Website.ProductRibbon VariantRibbon { get; set; }
    public Website.Website Website { get; set; }
    public List<Website.ProductImage> ProductVariantImages { get; set; }
    public double BaseUnitCount { get; set; }
    public Website.BaseUnit BaseUnitId { get; set; }
    public decimal BaseUnitPrice { get; set; }
    public string BaseUnitName { get; set; }
    public string WebsiteUrl { get; set; }

    public void ComputeBaseUnitPrice()
    {
        this.BaseUnitPrice = this._GetBaseUnitPrice(this.LstPrice);
    }

    public void ComputeBaseUnitName()
    {
        this.BaseUnitName = this.BaseUnitId.Name ?? this.UomName;
    }

    public void ComputeProductWebsiteUrl()
    {
        var attributes = string.Join(",", this.ProductTemplateAttributeValues.Select(x => x.ProductAttributeValue.Id.ToString()));
        this.WebsiteUrl = $"{this.ProductTemplate.WebsiteUrl}#attribute_values={attributes}";
    }

    private decimal _GetBaseUnitPrice(decimal price)
    {
        return this.BaseUnitCount != 0 ? price / this.BaseUnitCount : 0;
    }

    public void CheckBaseUnitCount()
    {
        if (this.BaseUnitCount < 0)
        {
            throw new Exception("The value of Base Unit Count must be greater than 0. Use 0 to hide the price per unit on this product.");
        }
    }

    public Dictionary<string, object> PrepareVariantValues(Dictionary<string, object> combination)
    {
        var variantDict = base.PrepareVariantValues(combination);
        variantDict["BaseUnitCount"] = this.BaseUnitCount;
        return variantDict;
    }

    public void WebsitePublishButton()
    {
        this.ProductTemplate.WebsitePublishButton();
    }

    public Dictionary<string, object> OpenWebsiteUrl()
    {
        var res = this.ProductTemplate.OpenWebsiteUrl();
        res["Url"] = this.WebsiteUrl;
        return res;
    }

    public List<object> GetImages()
    {
        var variantImages = this.ProductVariantImages.ToList();
        var templateImages = this.ProductTemplate.ProductTemplateImages.ToList();
        return new List<object> { this }.Concat(variantImages).Concat(templateImages).ToList();
    }

    public Dictionary<string, object> GetCombinationInfoVariant()
    {
        return this.ProductTemplate.GetCombinationInfo(combination: this.ProductTemplateAttributeValues, productId: this.Id);
    }

    public bool WebsiteShowQuickAdd()
    {
        var website = Env.Get<Website.Website>().GetCurrentWebsite();
        return this.SaleOk && (!website.PreventZeroPriceSale || this.GetContextualPrice());
    }

    public bool IsAddToCartAllowed()
    {
        var isProductSalable = this.Active && this.SaleOk && this.WebsitePublished;
        var website = Env.Get<Website.Website>().GetCurrentWebsite();
        return (isProductSalable && website.HasEcommerceAccess()) || Env.User.HasGroup("base.group_system");
    }

    private decimal LstPrice { get; set; }
    private string UomName { get; set; }
    private bool SaleOk { get; set; }
    private bool Active { get; set; }
    private bool WebsitePublished { get; set; }
    private Website.ProductTemplate ProductTemplate { get; set; }
    private List<Website.ProductTemplateAttributeValue> ProductTemplateAttributeValues { get; set; }

    private decimal GetContextualPrice()
    {
        // Placeholder for getting contextual price
        return 0;
    }
}
