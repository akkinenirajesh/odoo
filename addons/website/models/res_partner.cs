csharp
public partial class WebsiteResPartner 
{
    public string GoogleMapImg(int zoom = 8, int width = 298, int height = 298)
    {
        var googleMapsApiKey = Env.Get("website").Get("google_maps_api_key");
        if (string.IsNullOrEmpty(googleMapsApiKey))
        {
            return null;
        }
        var params = new Dictionary<string, string>
        {
            { "center", $"{this.Street ?? ""}, {this.City ?? ""}, {this.Zip ?? ""}, {this.Country != null ? this.Country.DisplayName : ""}" },
            { "size", $"{width}x{height}" },
            { "zoom", zoom.ToString() },
            { "sensor", "false" },
            { "key", googleMapsApiKey },
        };
        return $"//maps.googleapis.com/maps/api/staticmap?{string.Join("&", params.Select(x => $"{x.Key}={x.Value}"))}";
    }

    public string GoogleMapLink(int zoom = 10)
    {
        var params = new Dictionary<string, string>
        {
            { "q", $"{this.Street ?? ""}, {this.City ?? ""}, {this.Zip ?? ""}, {this.Country != null ? this.Country.DisplayName : ""}" },
            { "z", zoom.ToString() },
        };
        return $"https://maps.google.com/maps?{string.Join("&", params.Select(x => $"{x.Key}={x.Value}"))}";
    }

    public void ComputeDisplayName()
    {
        if (!Env.Context.Get("display_website") || !Env.User.IsInGroup("website.group_multi_website"))
        {
            return;
        }
        if (this.WebsiteId != null)
        {
            this.DisplayName += $" [{this.WebsiteId.Name}]";
        }
    }
}
