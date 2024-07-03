csharp
public partial class ResPartner
{
    public override bool Write(Dictionary<string, object> vals)
    {
        // Reset latitude/longitude in case we modify the address without
        // updating the related geolocation fields
        if (vals.Keys.Any(field => new[] { "Street", "Zip", "City", "StateId", "CountryId" }.Contains(field))
            && !new[] { "PartnerLatitude", "PartnerLongitude" }.All(field => vals.ContainsKey(field)))
        {
            vals["PartnerLatitude"] = 0.0;
            vals["PartnerLongitude"] = 0.0;
        }
        return base.Write(vals);
    }

    public (double? Latitude, double? Longitude) GeoLocalize(string street = "", string zip = "", string city = "", string state = "", string country = "")
    {
        var geoObj = Env.Get<BaseGeocoder>();
        var search = geoObj.GeoQueryAddress(street, zip, city, state, country);
        var result = geoObj.GeoFind(search, forceCountry: country);
        if (result == null)
        {
            search = geoObj.GeoQueryAddress(city: city, state: state, country: country);
            result = geoObj.GeoFind(search, forceCountry: country);
        }
        return result;
    }

    public bool GeoLocalize()
    {
        if (!Context.GetValueOrDefault("ForceGeoLocalize", false)
            && (Context.GetValueOrDefault("ImportFile", false)
                || Config.Any(key => new[] { "TestEnable", "TestFile", "Init", "Update" }.Contains(key.Key) && (bool)key.Value)))
        {
            return false;
        }

        var partnersNotGeoLocalized = new List<ResPartner>();
        var partner = this.WithContext(new Dictionary<string, object> { { "Lang", "en_US" } });

        var result = GeoLocalize(partner.Street, partner.Zip, partner.City, partner.State?.Name, partner.Country?.Name);

        if (result.Latitude.HasValue && result.Longitude.HasValue)
        {
            Write(new Dictionary<string, object>
            {
                { "PartnerLatitude", result.Latitude.Value },
                { "PartnerLongitude", result.Longitude.Value },
                { "DateLocalization", DateTime.Today }
            });
        }
        else
        {
            partnersNotGeoLocalized.Add(this);
        }

        if (partnersNotGeoLocalized.Any())
        {
            Env.Get<BusBus>().SendOne(Env.User.Partner, "SimpleNotification", new
            {
                Type = "danger",
                Title = "Warning",
                Message = $"No match found for {string.Join(", ", partnersNotGeoLocalized.Select(p => p.Name))} address(es)."
            });
        }

        return true;
    }
}
