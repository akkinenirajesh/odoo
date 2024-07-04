csharp
public partial class SaleOrder
{
    public void ActionConfirm()
    {
        var unmatch = Env.SaleOrder.Search(new []
        {
            ("Id", "in", new[] { this.Id }),
            ("Carrier.IsMondialrelay", "!=", "PartnerShipping.IsMondialrelay")
        });

        if (unmatch.Any())
        {
            string error = "Mondial Relay mismatching between delivery method and shipping address.";
            if (unmatch.Count() > 1)
            {
                error += $" ({string.Join(",", unmatch.Select(so => so.Name))})";
            }
            throw new UserException(error);
        }

        base.ActionConfirm();
    }
}
