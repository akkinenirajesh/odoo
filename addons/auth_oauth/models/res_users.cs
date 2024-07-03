csharp
public partial class ResUsers
{
    public string AuthOAuthRpc(string endpoint, string accessToken)
    {
        // Implementation of _auth_oauth_rpc method
        // Use C# HttpClient or similar to make API calls
        throw new NotImplementedException();
    }

    public Dictionary<string, object> AuthOAuthValidate(int provider, string accessToken)
    {
        // Implementation of _auth_oauth_validate method
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GenerateSignupValues(int provider, Dictionary<string, object> validation, Dictionary<string, string> parameters)
    {
        // Implementation of _generate_signup_values method
        throw new NotImplementedException();
    }

    public string AuthOAuthSignin(int provider, Dictionary<string, object> validation, Dictionary<string, string> parameters)
    {
        // Implementation of _auth_oauth_signin method
        throw new NotImplementedException();
    }

    public (string, string, string) AuthOAuth(int provider, Dictionary<string, string> parameters)
    {
        // Implementation of auth_oauth method
        throw new NotImplementedException();
    }

    public void CheckCredentials(string password, IEnvironment env)
    {
        // Implementation of _check_credentials method
        throw new NotImplementedException();
    }

    public HashSet<string> GetSessionTokenFields()
    {
        // Implementation of _get_session_token_fields method
        throw new NotImplementedException();
    }
}
