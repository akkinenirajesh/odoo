csharp
public partial class WebsiteSaleStockWishlist.ProductWishlist 
{
    public bool StockNotification { get; set; }
    public Product.Product ProductId { get; set; }
    public Res.Partner PartnerId { get; set; }

    public void ComputeStockNotification()
    {
        this.StockNotification = this.ProductId.HasStockNotification(this.PartnerId);
    }

    public void InverseStockNotification()
    {
        if (this.StockNotification)
        {
            this.ProductId.StockNotificationPartnerIds.Add(this.PartnerId);
        }
    }
}
