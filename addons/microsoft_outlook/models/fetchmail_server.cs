csharp
public partial class FetchmailServer
{
    // All methods related to FetchmailServer model

    public void ComputeServerTypeInfo()
    {
        if (this.ServerType == "outlook")
        {
            this.ServerTypeInfo = "Connect your personal Outlook account using OAuth.\nYou will be redirected to the Outlook login page to accept the permissions.";
        }
        else
        {
            // Call super method for other server types
        }
    }

    public void ComputeIsMicrosoftOutlookConfigured()
    {
        if (this.ServerType == "outlook")
        {
            // Logic for computing IsMicrosoftOutlookConfigured
        }
        else
        {
            this.IsMicrosoftOutlookConfigured = false;
        }
    }

    public void CheckUseMicrosoftOutlookService()
    {
        if (this.ServerType == "outlook" && !this.IsSsl)
        {
            throw new Exception("SSL is required for server " + this.Name);
        }
    }

    public void OnChangeServerType()
    {
        if (this.ServerType == "outlook")
        {
            this.Server = "imap.outlook.com";
            this.IsSsl = true;
            this.Port = 993;
        }
        else
        {
            // Clear Outlook specific fields
            this.MicrosoftOutlookRefreshToken = "";
            this.MicrosoftOutlookAccessToken = "";
            this.MicrosoftOutlookAccessTokenExpiration = null;
            // Call super method for other server types
        }
    }

    public void ImapLogin()
    {
        if (this.ServerType == "outlook")
        {
            // Authenticate using OAuth2
            // ...
        }
        else
        {
            // Call super method for other server types
        }
    }

    public string GetConnectionType()
    {
        if (this.ServerType == "outlook")
        {
            return "imap";
        }
        else
        {
            // Call super method for other server types
        }
    }
}
