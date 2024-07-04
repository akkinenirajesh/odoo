C#
public partial class WebsiteSaleStock.ProductTemplate
{
    public bool IsSoldOut()
    {
        if (this.IsStorable && this.ProductVariantId.IsSoldOut())
        {
            return true;
        }
        return false;
    }

    public bool WebsiteShowQuickAdd()
    {
        if (this.AllowOutOfStockOrder || !this.IsSoldOut())
        {
            return true;
        }
        return false;
    }

    public Dictionary<string, object> GetAdditionnalCombinationInfo(WebsiteSaleStock.ProductTemplate productOrTemplate, int quantity, DateTime date, Website website)
    {
        Dictionary<string, object> res = base.GetAdditionnalCombinationInfo(productOrTemplate, quantity, date, website);

        productOrTemplate = productOrTemplate.sudo();
        res.Add("ProductType", productOrTemplate.Type);
        res.Add("AllowOutOfStockOrder", productOrTemplate.AllowOutOfStockOrder);
        res.Add("AvailableThreshold", productOrTemplate.AvailableThreshold);
        if (productOrTemplate.IsProductVariant)
        {
            WebsiteSaleStock.Product product = productOrTemplate;
            int freeQty = website.GetProductAvailableQty(product);
            bool hasStockNotification = product.HasStockNotification(Env.User.PartnerId) || (Env.Request != null && product.Id.HasValue && Env.Request.Session.ContainsKey("product_with_stock_notification_enabled") && Env.Request.Session["product_with_stock_notification_enabled"] as Set<int> != null && (Env.Request.Session["product_with_stock_notification_enabled"] as Set<int>).Contains(product.Id.Value));
            string stockNotificationEmail = Env.Request != null ? Env.Request.Session.Get("stock_notification_email") : "";
            res.Add("FreeQty", freeQty);
            res.Add("CartQty", product.GetCartQty(website));
            res.Add("UomName", product.UomId.Name);
            res.Add("UomRounding", product.UomId.Rounding);
            res.Add("ShowAvailability", productOrTemplate.ShowAvailability);
            res.Add("OutOfStockMessage", productOrTemplate.OutOfStockMessage);
            res.Add("HasStockNotification", hasStockNotification);
            res.Add("StockNotificationEmail", stockNotificationEmail);
        }
        else
        {
            res.Add("FreeQty", 0);
            res.Add("CartQty", 0);
        }

        return res;
    }
}
