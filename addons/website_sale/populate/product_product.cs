C#
public partial class WebsiteSale.ProductProduct {
    public virtual bool IsPublished { get; set; }

    public List<WebsiteSale.ProductProduct> _populate_get_product_factories() {
        List<WebsiteSale.ProductProduct> result = Env.Model("WebsiteSale.ProductProduct")._populate_get_product_factories();
        result.Add(new WebsiteSale.ProductProduct() { IsPublished = Env.Utils.Randomize(new List<bool>() { true, false }, new List<int>() { 8, 2 }) });
        return result;
    }
}
