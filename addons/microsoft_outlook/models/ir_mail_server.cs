csharp
public partial class IrMailServer {

    public void ComputeIsMicrosoftOutlookConfigured() {
        if (this.SmtpAuthentication == "outlook") {
            this.IsMicrosoftOutlookConfigured = true;
        } else {
            this.IsMicrosoftOutlookConfigured = false;
        }
    }

    public void ComputeSmtpAuthenticationInfo() {
        if (this.SmtpAuthentication == "outlook") {
            this.SmtpAuthenticationInfo = "Connect your Outlook account with the OAuth Authentication process.  \nBy default, only a user with a matching email address will be able to use this server. To extend its use, you should set a \"mail.default.from\" system parameter.";
        }
    }

    public void OnChangeSmtpAuthenticationOutlook() {
        if (this.SmtpAuthentication == "outlook") {
            this.SmtpHost = "smtp.outlook.com";
            this.SmtpEncryption = "starttls";
            this.SmtpPort = 587;
        } else {
            this.MicrosoftOutlookRefreshToken = "";
            this.MicrosoftOutlookAccessToken = "";
            this.MicrosoftOutlookAccessTokenExpiration = null;
        }
    }

    public void OnChangeSmtpUserOutlook() {
        if (this.SmtpAuthentication == "outlook") {
            this.FromFilter = this.SmtpUser;
        }
    }

    public string GenerateOutlookOAuth2String(string SmtpUser) {
        // Implement logic to generate OAuth2 string
        // ...
        return "";
    }

    public void SmtpLogin(string connection, string SmtpUser, string SmtpPassword) {
        if (this.SmtpAuthentication == "outlook") {
            string authString = this.GenerateOutlookOAuth2String(SmtpUser);
            string oauthParam = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
            // Implement logic to authenticate using OAuth2
            // ...
        } else {
            // Call base implementation for other authentication types
            // ...
        }
    }

    public void CheckUseMicrosoftOutlookService() {
        if (this.SmtpAuthentication == "outlook") {
            if (!string.IsNullOrEmpty(this.SmtpPassword)) {
                throw new Exception("Please leave the password field empty for Outlook mail server " + this.Name + ". The OAuth process does not require it.");
            }

            if (this.SmtpEncryption != "starttls") {
                throw new Exception("Incorrect Connection Security for Outlook mail server " + this.Name + ". Please set it to \"TLS (STARTTLS)\".");
            }

            if (string.IsNullOrEmpty(this.SmtpUser)) {
                throw new Exception("Please fill the \"Username\" field with your Outlook/Office365 username (your email address). This should be the same account as the one used for the Outlook OAuthentication Token.");
            }
        }
    }

    public void OnChangeEncryption() {
        if (this.SmtpAuthentication != "outlook") {
            // Call base implementation for non-Outlook servers
            // ...
        }
    }
}
