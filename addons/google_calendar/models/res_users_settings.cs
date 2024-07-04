csharp
public partial class ResUsersSettings
{
    public void SetGoogleAuthTokens(string accessToken, string refreshToken, int ttl)
    {
        GoogleCalendarRToken = refreshToken;
        GoogleCalendarToken = accessToken;
        GoogleCalendarTokenValidity = ttl > 0 ? DateTime.Now.AddSeconds(ttl) : (DateTime?)null;
    }

    public bool GoogleCalendarAuthenticated()
    {
        return !string.IsNullOrEmpty(GoogleCalendarRToken);
    }

    public bool IsGoogleCalendarValid()
    {
        return GoogleCalendarTokenValidity.HasValue && GoogleCalendarTokenValidity.Value >= DateTime.Now.AddMinutes(1);
    }

    public void RefreshGoogleCalendarToken()
    {
        var configParam = Env.GetService<IConfigurationParameterService>();
        string clientId = configParam.GetParam("google_calendar_client_id");
        string clientSecret = configParam.GetParam("google_calendar_client_secret");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new UserException("The account for the Google Calendar service is not configured.");
        }

        var data = new Dictionary<string, string>
        {
            ["refresh_token"] = GoogleCalendarRToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token"
        };

        try
        {
            var googleService = Env.GetService<IGoogleService>();
            var response = googleService.DoRequest(GoogleService.GOOGLE_TOKEN_ENDPOINT, data, method: "POST");
            
            int ttl = response.GetValueOrDefault("expires_in", 0);
            GoogleCalendarToken = response.GetValueOrDefault("access_token", "");
            GoogleCalendarTokenValidity = DateTime.Now.AddSeconds(ttl);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest || ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Env.Rollback();
                SetGoogleAuthTokens(null, null, 0);
                Env.Commit();
            }

            string errorKey = ex.Data.Contains("error") ? ex.Data["error"].ToString() : "nc";
            string errorMsg = $"An error occurred while generating the token. Your authorization code may be invalid or has already expired [{errorKey}]. " +
                              "You should check your Client ID and secret on the Google APIs plateform or try to stop and restart your calendar synchronization.";
            throw new UserException(errorMsg);
        }
    }
}
