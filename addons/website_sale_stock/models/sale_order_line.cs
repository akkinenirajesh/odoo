csharp
public partial class WebsiteSaleStock.SaleOrderLine
{
    public string SetShopWarningStock(decimal desiredQty, decimal newQty)
    {
        this.ShopWarning = Env.Translate("You ask for {0} products but only {1} is available", desiredQty, newQty);
        return this.ShopWarning;
    }

    public decimal GetMaxAvailableQty()
    {
        return this.Product.FreeQty - this.Product.GetCartQty();
    }
}
