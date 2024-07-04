csharp
public partial class WebsiteMondialRelay
{
    public Dictionary<string, object> PrepareSaleOrderValues(Partner partnerSudo)
    {
        var values = base.PrepareSaleOrderValues(partnerSudo);

        // never use Mondial Relay shipping address as default.
        var shippingAddress = Env.Get<Partner>().Browse((int)values["PartnerShippingId"]);
        if (shippingAddress.Id != (int)values["PartnerInvoiceId"] && shippingAddress.IsMondialrelay)
        {
            values["PartnerShippingId"] = values["PartnerInvoiceId"];
        }
        return values;
    }
}
