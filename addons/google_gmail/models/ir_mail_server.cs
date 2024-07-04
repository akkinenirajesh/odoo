csharp
public partial class IrMailServer
{
    public void OnChangeEncryption()
    {
        if (SmtpAuthentication != SmtpAuthenticationType.Gmail)
        {
            // Implement the logic for changing encryption settings
            // This would be similar to the super() call in Python
        }
    }

    public void OnChangeSmtpAuthenticationGmail()
    {
        if (SmtpAuthentication == SmtpAuthenticationType.Gmail)
        {
            SmtpHost = "smtp.gmail.com";
            SmtpEncryption = "starttls";
            SmtpPort = 587;
        }
        else
        {
            GoogleGmailAuthorizationCode = null;
            GoogleGmailRefreshToken = null;
            GoogleGmailAccessToken = null;
            GoogleGmailAccessTokenExpiration = null;
        }
    }

    public void OnChangeSmtpUserGmail()
    {
        if (SmtpAuthentication == SmtpAuthenticationType.Gmail)
        {
            FromFilter = SmtpUser;
        }
    }

    public void CheckUseGoogleGmailService()
    {
        if (SmtpAuthentication == SmtpAuthenticationType.Gmail)
        {
            if (!string.IsNullOrEmpty(SmtpPass))
            {
                throw new UserErrorException($"Please leave the password field empty for Gmail mail server \"{Name}\". The OAuth process does not require it");
            }

            if (SmtpEncryption != "starttls")
            {
                throw new UserErrorException($"Incorrect Connection Security for Gmail mail server \"{Name}\". Please set it to \"TLS (STARTTLS)\".");
            }

            if (string.IsNullOrEmpty(SmtpUser))
            {
                throw new UserErrorException("Please fill the \"Username\" field with your Gmail username (your email address). This should be the same account as the one used for the Gmail OAuthentication Token.");
            }
        }
    }

    public void SmtpLogin(SmtpConnection connection, string smtpUser, string smtpPassword)
    {
        if (SmtpAuthentication == SmtpAuthenticationType.Gmail)
        {
            string authString = GenerateOAuth2String(smtpUser, GoogleGmailRefreshToken);
            string oauthParam = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authString));
            connection.Ehlo();
            connection.Docmd("AUTH", $"XOAUTH2 {oauthParam}");
        }
        else
        {
            // Implement the base SMTP login logic here
        }
    }

    private string GenerateOAuth2String(string smtpUser, string refreshToken)
    {
        // Implement the OAuth2 string generation logic here
        throw new NotImplementedException();
    }

    private void _ComputeSmtpAuthenticationInfo()
    {
        if (SmtpAuthentication == SmtpAuthenticationType.Gmail)
        {
            SmtpAuthenticationInfo = "Connect your Gmail account with the OAuth Authentication process.\n" +
                "By default, only a user with a matching email address will be able to use this server. " +
                "To extend its use, you should set a \"mail.default.from\" system parameter.";
        }
        else
        {
            // Compute authentication info for other types
        }
    }
}
