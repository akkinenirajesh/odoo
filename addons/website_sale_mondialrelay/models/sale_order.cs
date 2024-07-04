csharp
public partial class WebsiteSaleMondialRelay.SaleOrder
{
    public void CheckCartIsReadyToBePaid()
    {
        if (this.PartnerShippingId.IsMondialRelay && this.DeliverySet && this.CarrierId && !this.CarrierId.IsMondialRelay)
        {
            throw new Exception("Point Relais® can only be used with the delivery method Mondial Relay.");
        }
        else if (!this.PartnerShippingId.IsMondialRelay && this.CarrierId.IsMondialRelay)
        {
            throw new Exception("Delivery method Mondial Relay can only ship to Point Relais®.");
        }
        Env.Call("super", "_check_cart_is_ready_to_be_paid");
    }
}
