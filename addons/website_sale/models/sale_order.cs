C#
public partial class SaleOrder {
    public void ComputeWebsiteOrderLine() {
        // implement your logic here
        this.WebsiteOrderLine = this.Env.Get<SaleOrderLine>().Search(x => x.ShowInCart());
    }

    public void ComputeAmountDelivery() {
        // implement your logic here
        var deliveryLines = this.Env.Get<SaleOrderLine>().Search(x => x.IsDelivery);
        if (this.WebsiteId.ShowLineSubtotalsTaxSelection == "TaxExcluded") {
            this.AmountDelivery = deliveryLines.Sum(x => x.PriceSubtotal);
        } else {
            this.AmountDelivery = deliveryLines.Sum(x => x.PriceTotal);
        }
    }

    public void ComputeCartInfo() {
        // implement your logic here
        this.CartQuantity = this.Env.Get<SaleOrderLine>().Search(x => x.ShowInCart()).Sum(x => x.ProductUomQty);
        this.OnlyServices = this.Env.Get<SaleOrderLine>().Search(x => x.ShowInCart()).All(x => x.Product.Type == "service");
    }

    public void ComputeAbandonedCart() {
        // implement your logic here
        if (this.WebsiteId != null && this.State == "draft" && this.DateOrder != null) {
            var publicPartnerId = this.WebsiteId.UserId.Partner.Id;
            var abandonedDelay = this.WebsiteId.CartAbandonedDelay ?? 1.0;
            var abandonedDateTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(abandonedDelay));
            this.IsAbandonedCart = this.DateOrder <= abandonedDateTime && this.Partner != publicPartnerId && this.Env.Get<SaleOrderLine>().Search(x => x.Id != 0).Count() > 0;
        } else {
            this.IsAbandonedCart = false;
        }
    }

    public Domain SearchAbandonedCart(string operator_, object value) {
        // implement your logic here
        return null;
    }

}
