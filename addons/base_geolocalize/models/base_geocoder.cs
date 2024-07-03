csharp
public partial class GeoCoder
{
    public GeoProvider GetProvider()
    {
        var provId = Env.GetParameter("base_geolocalize.geo_provider");
        GeoProvider provider = null;
        if (!string.IsNullOrEmpty(provId))
        {
            provider = Env.GeoProviders.FirstOrDefault(p => p.Id == int.Parse(provId));
        }
        if (provider == null)
        {
            provider = Env.GeoProviders.FirstOrDefault();
        }
        return provider;
    }

    public string GeoQueryAddress(string street = null, string zip = null, string city = null, string state = null, string country = null)
    {
        var provider = GetProvider().TechName;
        var methodName = $"GeoQueryAddress{provider}";
        var method = GetType().GetMethod(methodName);
        if (method != null)
        {
            return (string)method.Invoke(this, new object[] { street, zip, city, state, country });
        }
        else
        {
            return GeoQueryAddressDefault(street, zip, city, state, country);
        }
    }

    public (double? Latitude, double? Longitude) GeoFind(string addr, Dictionary<string, object> kw = null)
    {
        var provider = GetProvider().TechName;
        try
        {
            var methodName = $"Call{provider}";
            var method = GetType().GetMethod(methodName);
            if (method == null)
            {
                throw new UserException($"Provider {provider} is not implemented for geolocation service.");
            }
            return ((double? Latitude, double? Longitude))method.Invoke(this, new object[] { addr, kw });
        }
        catch (UserException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Env.Logger.Debug("Geolocalize call failed", ex);
            return (null, null);
        }
    }

    public (double? Latitude, double? Longitude) CallOpenstreetmap(string addr, Dictionary<string, object> kw = null)
    {
        if (string.IsNullOrEmpty(addr))
        {
            Env.Logger.Info("Invalid address given");
            return (null, null);
        }

        var url = "https://nominatim.openstreetmap.org/search";
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Odoo (http://www.odoo.com/contactus)");
            var response = client.GetAsync($"{url}?format=json&q={Uri.EscapeDataString(addr)}").Result;
            Env.Logger.Info("Openstreetmap nominatim service called");
            if (!response.IsSuccessStatusCode)
            {
                Env.Logger.Warning($"Request to openstreetmap failed.\nCode: {response.StatusCode}\nContent: {response.Content.ReadAsStringAsync().Result}");
            }
            var result = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Content.ReadAsStringAsync().Result);
            var geo = result[0];
            return (double.Parse(geo["lat"]), double.Parse(geo["lon"]));
        }
        catch (Exception ex)
        {
            RaiseQueryError(ex);
            return (null, null);
        }
    }

    // Additional methods like CallGooglemap, GeoQueryAddressDefault, etc. would be implemented similarly

    private void RaiseQueryError(Exception error)
    {
        throw new UserException($"Error with geolocation server: {error.Message}");
    }
}
