csharp
public partial class Website
{
    public bool HasGooglePlacesApiKey()
    {
        return !string.IsNullOrEmpty(this.GooglePlacesApiKey);
    }
}
