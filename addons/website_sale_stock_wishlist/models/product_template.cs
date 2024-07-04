csharp
public partial class WebsiteSaleStockWishlist.ProductTemplate {
    public virtual object _get_additionnal_combination_info(object productOrTemplate, int quantity, DateTime date, object website) {
        var res = Env.Call("WebsiteSaleStockWishlist.ProductTemplate", "_get_additionnal_combination_info", productOrTemplate, quantity, date, website);
        if (!Env.Context.ContainsKey("website_sale_stock_wishlist_get_wish")) {
            return res;
        }
        if ((bool)this.IsProductVariant) {
            var productSudo = Env.Call("WebsiteSaleStockWishlist.ProductTemplate", "sudo", this);
            res["is_in_wishlist"] = Env.Call("WebsiteSaleStockWishlist.ProductTemplate", "_is_in_wishlist", productSudo);
        }
        return res;
    }
}
