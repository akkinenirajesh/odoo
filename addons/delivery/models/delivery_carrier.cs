csharp
public partial class DeliveryCarrier
{
    public override string ToString()
    {
        return Name;
    }

    public bool ToggleProdEnvironment()
    {
        ProdEnvironment = !ProdEnvironment;
        return ProdEnvironment;
    }

    public bool ToggleDebug()
    {
        DebugLogging = !DebugLogging;
        return DebugLogging;
    }

    public bool IsAvailableForOrder(SalesOrder order)
    {
        if (!Match(order.PartnerShipping, order))
            return false;

        if (DeliveryType == DeliveryType.BaseOnRule)
            return RateShipment(order).Success;

        return true;
    }

    public List<DeliveryCarrier> AvailableCarriers(Partner partner, SalesOrder order)
    {
        return Env.DeliveryCarrier.All().Where(c => c.Match(partner, order)).ToList();
    }

    private bool Match(Partner partner, SalesOrder order)
    {
        return MatchAddress(partner) && 
               MatchMustHaveTags(order) && 
               MatchExcludedTags(order) && 
               MatchWeight(order) && 
               MatchVolume(order);
    }

    private bool MatchAddress(Partner partner)
    {
        if (Countries.Any() && !Countries.Contains(partner.Country))
            return false;
        if (States.Any() && !States.Contains(partner.State))
            return false;
        if (ZipPrefixes.Any())
        {
            var regex = new Regex(string.Join("|", ZipPrefixes.Select(zp => "^" + zp.Name)));
            if (string.IsNullOrEmpty(partner.Zip) || !regex.IsMatch(partner.Zip.ToUpper()))
                return false;
        }
        return true;
    }

    private bool MatchMustHaveTags(SalesOrder order)
    {
        return MustHaveTags.All(tag => order.OrderLines.Any(line => line.Product.AllProductTags.Contains(tag)));
    }

    private bool MatchExcludedTags(SalesOrder order)
    {
        return !ExcludedTags.Any(tag => order.OrderLines.Any(line => line.Product.AllProductTags.Contains(tag)));
    }

    private bool MatchWeight(SalesOrder order)
    {
        if (!MaxWeight.HasValue)
            return true;
        return order.OrderLines.Sum(line => line.Product.Weight * line.ProductQty) <= MaxWeight.Value;
    }

    private bool MatchVolume(SalesOrder order)
    {
        if (!MaxVolume.HasValue)
            return true;
        return order.OrderLines.Sum(line => line.Product.Volume * line.ProductQty) <= MaxVolume.Value;
    }

    public float ApplyMargins(float price)
    {
        if (DeliveryType == DeliveryType.Fixed)
            return price;
        return price * (1.0f + Margin) + FixedMargin;
    }

    public ShipmentRate RateShipment(SalesOrder order)
    {
        // Implementation depends on the specific delivery type
        throw new NotImplementedException();
    }

    public void LogXml(string xmlString, string func)
    {
        if (!DebugLogging)
            return;

        // Implementation of logging logic
        throw new NotImplementedException();
    }
}
