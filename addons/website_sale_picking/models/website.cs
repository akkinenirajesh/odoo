csharp
public partial class Website {
    public void ComputePickingSites() {
        var deliveryCarriers = Env.Search<DeliveryCarrier>(x => x.DeliveryType == "onsite");
        this.PickingSites = deliveryCarriers.Where(x => x.WebsiteId == this.Id).ToList();
    }
}
