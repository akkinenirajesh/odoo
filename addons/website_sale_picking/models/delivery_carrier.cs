csharp
public partial class DeliveryCarrier {

    public decimal OnsiteRateShipment(Order order) {
        return this.ProductId.ListPrice;
    }

    public List<ShippingInfo> OnsiteSendShipping(List<Picking> pickings) {
        return pickings.Select(p => new ShippingInfo { ExactPrice = p.CarrierId.FixedPrice, TrackingNumber = null }).ToList();
    }

    public void OnsiteCancelShipment(List<Picking> pickings) { 
        // No need to communicate to an external service, however the method must exist so that cancel_shipment() works.
    }

    private class ShippingInfo {
        public decimal ExactPrice { get; set; }
        public string TrackingNumber { get; set; }
    }
}
