csharp
public partial class ResPartnerMondialRelay
{
    public void _ComputeIsMondialRelay()
    {
        this.IsMondialRelay = !string.IsNullOrEmpty(this.Ref) && this.Ref.StartsWith("MR#");
    }

    public ResPartnerMondialRelay MondialRelaySearchOrCreate(Dictionary<string, object> data)
    {
        string refValue = $"MR#{data["id"]}";
        var partner = Env.ResPartner.Search(new[]
        {
            ("Id", "ChildOf", this.CommercialPartnerId.Ids),
            ("Ref", "=", refValue),
            ("Street", "=", data["street"]),
            ("Zip", "=", data["zip"])
        }).FirstOrDefault();

        if (partner == null)
        {
            partner = Env.ResPartner.Create(new Dictionary<string, object>
            {
                {"Ref", refValue},
                {"Name", data["name"]},
                {"Street", data["street"]},
                {"Street2", data["street2"]},
                {"Zip", data["zip"]},
                {"City", data["city"]},
                {"CountryId", Env.Ref($"base.{data["country_code"]}").Id},
                {"Type", "delivery"},
                {"ParentId", this.Id}
            });
        }

        return partner;
    }

    public override string AvatarGetPlaceholderPath()
    {
        if (this.IsMondialRelay)
        {
            return "delivery_mondialrelay/static/src/img/truck_mr.png";
        }
        return base.AvatarGetPlaceholderPath();
    }
}
