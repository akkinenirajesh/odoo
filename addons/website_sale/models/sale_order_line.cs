csharp
public partial class WebsiteSale.SaleOrderLine {
    public string NameShort { get; set; }
    public string ShopWarning { get; set; }

    public void ComputeNameShort() {
        this.NameShort = this.Product.WithContext(new { DisplayDefaultCode = false }).DisplayName;
    }

    public List<string> GetDescriptionFollowingLines() {
        return this.Name.Split('\n').ToList().Skip(1).ToList();
    }

    public decimal GetPricelistPriceBeforeDiscount() {
        if (this.Order.Website) {
            return Env.Get<ProductPricelistItem>().ComputePriceBeforeDiscount(
                this.Product.WithContext(this.GetProductPriceContext()),
                this.ProductUomQty,
                this.ProductUom,
                this.Order.DateOrder,
                this.Currency
            );
        }
        return this.GetPricelistPriceBeforeDiscount();
    }

    public string GetShopWarning(bool clear = true) {
        string warn = this.ShopWarning;
        if (clear) {
            this.ShopWarning = "";
        }
        return warn;
    }

    public decimal GetDisplayedUnitPrice() {
        bool showTax = this.Order.Website.ShowLineSubtotalsTaxSelection;
        string taxDisplay = showTax == "tax_excluded" ? "total_excluded" : "total_included";
        return this.Tax.ComputeAll(
            this.PriceUnit,
            this.Currency,
            1,
            this.Product,
            this.OrderPartner,
        )[taxDisplay];
    }

    public bool ShowInCart() {
        return !this.IsDelivery && !this.DisplayType;
    }

    public bool IsReorderAllowed() {
        return this.Product.IsAddToCartAllowed();
    }
}
