csharp
public partial class Website.ResCountry
{
    public Website.ResCountry GetWebsiteSaleCountries(string mode = "billing")
    {
        var res = Env.Search<Website.ResCountry>();
        if (mode == "shipping")
        {
            var countries = Env.Search<Website.ResCountry>();

            var deliveryCarriers = Env.Search<Delivery.Carrier>(x => x.WebsitePublished == true);
            foreach (var carrier in deliveryCarriers)
            {
                if (carrier.Countries.Count == 0 && carrier.States.Count == 0)
                {
                    countries = res;
                    break;
                }
                countries = countries.Union(carrier.Countries);
            }
            res = res.Intersect(countries);
        }
        return res;
    }

    public Website.ResCountryState GetWebsiteSaleStates(string mode = "billing")
    {
        var res = this.States;
        if (mode == "shipping")
        {
            var states = Env.Search<Website.ResCountryState>();
            var dom = new[] { "|", ("Countries", "in", this.Id), ("Countries", "=", 0), ("WebsitePublished", "=", true) };
            var deliveryCarriers = Env.Search<Delivery.Carrier>(dom);

            foreach (var carrier in deliveryCarriers)
            {
                if (carrier.Countries.Count == 0 || carrier.States.Count == 0)
                {
                    states = res;
                    break;
                }
                states = states.Union(carrier.States);
            }
            res = res.Intersect(states);
        }
        return res;
    }
}
