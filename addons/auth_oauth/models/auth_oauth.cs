csharp
public partial class OAuthProvider
{
    public override string ToString()
    {
        return Name;
    }

    public string GetFullAuthorizationUrl()
    {
        // Example method to construct the full authorization URL
        return $"{AuthEndpoint}?client_id={ClientId}&scope={Scope}";
    }

    public bool IsValidProvider()
    {
        // Example method to check if the provider is valid and can be used
        return Enabled && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(AuthEndpoint) && !string.IsNullOrEmpty(ValidationEndpoint);
    }

    public void ToggleEnabled()
    {
        // Example method to toggle the Enabled status
        Enabled = !Enabled;
        Env.SaveChanges(); // Assuming Env is available to save changes
    }
}
