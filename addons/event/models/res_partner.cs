csharp
public partial class ResPartner
{
    public int ComputeEventCount()
    {
        return Env.Get<EventEvent>().SearchCount(new[] { ("RegistrationIds.PartnerId", "child_of", this.Id) });
    }

    public string ComputeStaticMapUrl()
    {
        return GoogleMapSignedImg(zoom: 13, width: 598, height: 200);
    }

    public bool ComputeStaticMapUrlIsValid()
    {
        if (string.IsNullOrEmpty(StaticMapUrl))
        {
            return false;
        }

        using (var client = new HttpClient())
        {
            try
            {
                var response = client.GetAsync(StaticMapUrl).Result;
                return response.IsSuccessStatusCode && !response.Headers.Contains("X-Staticmap-API-Warning");
            }
            catch
            {
                return false;
            }
        }
    }

    public ActionResult ActionEventView()
    {
        var action = Env.Get<IrActionsActions>().ForXmlId("Event.ActionEventView");
        action.Context = new Dictionary<string, object>();
        action.Domain = new[] { ("RegistrationIds.PartnerId", "child_of", this.Id) };
        return action;
    }

    private string GoogleMapSignedImg(int zoom = 13, int width = 298, int height = 298)
    {
        var apiKey = Env.Get<IrConfigParameter>().GetParam("google_maps.signed_static_api_key");
        var apiSecret = Env.Get<IrConfigParameter>().GetParam("google_maps.signed_static_api_secret");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            return null;
        }

        var locationString = $"{Street}, {City} {Zip}, {Country?.DisplayName ?? ""}";
        var parameters = new Dictionary<string, string>
        {
            ["center"] = locationString,
            ["markers"] = $"size:mid|{locationString}",
            ["size"] = $"{width}x{height}",
            ["zoom"] = zoom.ToString(),
            ["sensor"] = "false",
            ["key"] = apiKey
        };

        var unsignedPath = "/maps/api/staticmap?" + string.Join("&", parameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

        byte[] secretBytes;
        try
        {
            secretBytes = Convert.FromBase64String(apiSecret + new string('=', (4 - apiSecret.Length % 4) % 4));
        }
        catch
        {
            return null;
        }

        using (var hmac = new System.Security.Cryptography.HMACSHA1(secretBytes))
        {
            var signatureBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(unsignedPath));
            parameters["signature"] = Convert.ToBase64String(signatureBytes).Replace('+', '-').Replace('/', '_');
        }

        return "https://maps.googleapis.com/maps/api/staticmap?" + string.Join("&", parameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
    }
}
