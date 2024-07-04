csharp
public partial class WebsiteSaleComparison.ProductTemplateAttributeLine {

    public  OrderedDictionary<Product.Attribute.Category, WebsiteSaleComparison.ProductTemplateAttributeLine> PrepareCategoriesForDisplay() {
        // implementation here
        var attributes = Env.Get<Product.Attribute>().Browse(this.AttributeId);
        var categories = new OrderedDict<Product.Attribute.Category, WebsiteSaleComparison.ProductTemplateAttributeLine>();
        foreach (var cat in attributes.Category.Sorted()) {
            categories.Add(cat, Env.Get<WebsiteSaleComparison.ProductTemplateAttributeLine>());
        }

        if (attributes.Any(pa => pa.Category == null)) {
            categories.Add(Env.Get<Product.Attribute.Category>(), Env.Get<WebsiteSaleComparison.ProductTemplateAttributeLine>());
        }

        foreach (var ptal in this) {
            categories[ptal.AttributeId.Category] |= ptal;
        }
        return categories;
    }
}
