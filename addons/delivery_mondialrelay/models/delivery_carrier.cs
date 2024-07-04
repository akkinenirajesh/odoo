csharp
public partial class DeliveryCarrier
{
    public bool ComputeIsMondialrelay()
    {
        return this.ProductId?.DefaultCode == "MR";
    }

    public static IEnumerable<int> SearchIsMondialrelay(string @operator, bool value)
    {
        if (@operator != "=" && @operator != "!=")
        {
            throw new UserException("Operation not supported");
        }
        if (!value)
        {
            @operator = @operator == "=" ? "!=" : "=";
        }
        return Env.Query<DeliveryCarrier>()
            .Where(c => c.ProductId.DefaultCode.Equals("MR", @operator))
            .Select(c => c.Id);
    }

    public string FixedGetTrackingLink(StockPicking picking)
    {
        if (this.IsMondialrelay)
        {
            return this.BaseOnRuleGetTrackingLink(picking);
        }
        return base.FixedGetTrackingLink(picking);
    }

    public string BaseOnRuleGetTrackingLink(StockPicking picking)
    {
        if (this.IsMondialrelay)
        {
            return string.Format(
                "https://www.mondialrelay.com/public/permanent/tracking.aspx?ens={0}&exp={1}&language={2}",
                picking.CarrierId.MondialrelayBrand,
                picking.CarrierTrackingRef,
                (picking.PartnerId.Lang ?? "fr").Split('_')[0]
            );
        }
        return base.BaseOnRuleGetTrackingLink(picking);
    }
}
