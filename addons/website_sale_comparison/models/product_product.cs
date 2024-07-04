csharp
public partial class WebsiteSaleComparison_ProductProduct
{
    public WebsiteSaleComparison_ProductProduct(dynamic env)
    {
        Env = env;
    }

    public dynamic Env { get; private set; }

    public Dictionary<WebsiteSaleComparison_ProductAttributeCategory, Dictionary<WebsiteSaleComparison_ProductAttribute, Dictionary<WebsiteSaleComparison_ProductProduct, List<WebsiteSaleComparison_ProductAttributeValue>>>> PrepareCategoriesForDisplay()
    {
        var attributes = this.ProductTemplateId.ValidProductTemplateAttributeLineIds.AttributeId.OrderBy(x => x.Sequence).ToList();
        var categories = new Dictionary<WebsiteSaleComparison_ProductAttributeCategory, Dictionary<WebsiteSaleComparison_ProductAttribute, Dictionary<WebsiteSaleComparison_ProductProduct, List<WebsiteSaleComparison_ProductAttributeValue>>>>();
        foreach (var cat in attributes.Select(x => x.CategoryID).Distinct().OrderBy(x => x.Sequence))
        {
            categories.Add(cat, new Dictionary<WebsiteSaleComparison_ProductAttribute, Dictionary<WebsiteSaleComparison_ProductProduct, List<WebsiteSaleComparison_ProductAttributeValue>>>());
        }

        // category_id is not required and the mapped does not return empty
        if (attributes.Any(x => x.CategoryID == null))
        {
            categories.Add(Env["product.attribute.category"], new Dictionary<WebsiteSaleComparison_ProductAttribute, Dictionary<WebsiteSaleComparison_ProductProduct, List<WebsiteSaleComparison_ProductAttributeValue>>>());
        }

        foreach (var pa in attributes)
        {
            categories[pa.CategoryID].Add(pa, new Dictionary<WebsiteSaleComparison_ProductProduct, List<WebsiteSaleComparison_ProductAttributeValue>>());

            foreach (var product in Env["WebsiteSaleComparison.ProductProduct"].Search([]))
            {
                categories[pa.CategoryID][pa].Add(product, product.AttributeLineIds.Where(x => x.AttributeId == pa).SelectMany(x => x.ValueIds).ToList());
            }
        }

        return categories;
    }
}
