csharp
public partial class WebsiteSale_ProductTemplateAttributeLine {
    public WebsiteSale_ProductTemplateAttributeLine PrepareSingleValueForDisplay() {
        var singleValueLines = this.Env.Search<WebsiteSale_ProductTemplateAttributeLine>("(ValueIds.Count == 1) and (AttributeId.DisplayType != 'multi')");
        var singleValueAttributes = new Dictionary<Product_ProductAttribute, WebsiteSale_ProductTemplateAttributeLine>();
        foreach (var pa in singleValueLines.AttributeId) {
            singleValueAttributes.Add(pa, this.Env.Search<WebsiteSale_ProductTemplateAttributeLine>("AttributeId == " + pa));
        }
        foreach (var ptal in singleValueLines) {
            singleValueAttributes[ptal.AttributeId] |= ptal;
        }
        return singleValueAttributes;
    }
}
