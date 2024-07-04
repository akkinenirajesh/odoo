csharp
public partial class WebsiteEventSale.PricelistItem
{
    public void OnchangeEventSaleWarning()
    {
        if (this.MinQuantity > 0)
        {
            string msg = "";
            if (this.AppliedOn == "3_global" || this.AppliedOn == "2_product_category")
            {
                msg = Env.Translate("A pricelist item with a positive min. quantity will not be applied to the event tickets products.");
            }
            else if ((this.AppliedOn == "1_product" && this.ProductTemplateId.ServiceTracking == "event") ||
                     (this.AppliedOn == "0_product_variant" && this.ProductId.ServiceTracking == "event"))
            {
                msg = Env.Translate("A pricelist item with a positive min. quantity cannot be applied to this event tickets product.");
            }
            if (msg != "")
            {
                Env.Warning(new { Title = "Warning", Message = msg });
            }
        }
    }
}
