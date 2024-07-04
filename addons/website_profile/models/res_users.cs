csharp
public partial class WebsiteUsers
{
    public int Karma { get; set; }
    public Core.Country Country { get; set; }
    public string City { get; set; }
    public string Website { get; set; }
    public string WebsiteDescription { get; set; }
    public bool WebsitePublished { get; set; }

    public string GenerateProfileToken(int userId, string email)
    {
        string profileUuid = Env.GetParam("website_profile.uuid");
        if (string.IsNullOrEmpty(profileUuid))
        {
            profileUuid = Guid.NewGuid().ToString();
            Env.SetParam("website_profile.uuid", profileUuid);
        }
        return BitConverter.ToString(
            System.Security.Cryptography.SHA256.Create()
                .ComputeHash(
                    Encoding.UTF8.GetBytes(
                        $"{DateTime.Now.Date}-{profileUuid}-{userId}-{email}"))
        ).Replace("-", string.Empty);
    }

    public bool SendProfileValidationEmail(string token, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        var activationTemplate = Env.Ref("website_profile.validation_email");
        if (activationTemplate != null)
        {
            string tokenUrl = Env.GetBaseUrl() + "/profile/validate_email?" + HttpUtility.ParseQueryString(
                $"token={token}&user_id={this.Id}&email={email}");
            using (var transaction = Env.Transaction())
            {
                activationTemplate.SendMail(
                    this.Id,
                    forceSend: true,
                    raiseException: true,
                    tokenUrl: tokenUrl);
            }
            return true;
        }
        return false;
    }

    public bool ProcessProfileValidationToken(string token, string email)
    {
        if (token == GenerateProfileToken(this.Id, email) && Karma == 0)
        {
            return Env.Write(this, new { Karma = 3 });
        }
        return false;
    }
}
