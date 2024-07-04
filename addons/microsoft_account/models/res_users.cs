C#
public partial class ResUsers {
    public void SetMicrosoftAuthTokens(string accessToken, string refreshToken, int ttl) {
        this.MicrosoftCalendarRToken = refreshToken;
        this.MicrosoftCalendarToken = accessToken;
        this.MicrosoftCalendarTokenValidity = Env.Now + TimeSpan.FromSeconds(ttl);
    }
}
