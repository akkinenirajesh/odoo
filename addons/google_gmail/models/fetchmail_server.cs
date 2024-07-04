csharp
public partial class FetchmailServer
{
    public void ComputeServerTypeInfo()
    {
        if (ServerType == ServerType.Gmail)
        {
            ServerTypeInfo = "Connect your Gmail account with the OAuth Authentication process. \n" +
                             "You will be redirected to the Gmail login page where you will " +
                             "need to accept the permission.";
        }
        else
        {
            // Call the base implementation for other server types
            base.ComputeServerTypeInfo();
        }
    }

    public void OnChangeServerType()
    {
        if (ServerType == ServerType.Gmail)
        {
            Server = "imap.gmail.com";
            IsSsl = true;
            Port = 993;
        }
        else
        {
            GoogleGmailAuthorizationCode = null;
            GoogleGmailRefreshToken = null;
            GoogleGmailAccessToken = null;
            GoogleGmailAccessTokenExpiration = null;
            // Call the base implementation for other server types
            base.OnChangeServerType();
        }
    }

    public void ImapLogin(object connection)
    {
        if (ServerType == ServerType.Gmail)
        {
            string authString = GenerateOauth2String(User, GoogleGmailRefreshToken);
            // Implement the XOAUTH2 authentication here
            // This is a placeholder and needs to be implemented based on the specific requirements
            // connection.Authenticate("XOAUTH2", authString);
            // connection.Select("INBOX");
        }
        else
        {
            // Call the base implementation for other server types
            base.ImapLogin(connection);
        }
    }

    public string GetConnectionType()
    {
        return ServerType == ServerType.Gmail ? "imap" : base.GetConnectionType();
    }

    private string GenerateOauth2String(string user, string refreshToken)
    {
        // Implement the OAuth2 string generation logic here
        // This is a placeholder and needs to be implemented based on the specific requirements
        return "";
    }
}
