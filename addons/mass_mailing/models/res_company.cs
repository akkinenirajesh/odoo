C#
public partial class MassMailing.ResCompany 
{
    public Dictionary<string, string> GetSocialMediaLinks()
    {
        return new Dictionary<string, string>()
        {
            { "social_facebook", this.SocialFacebook },
            { "social_linkedin", this.SocialLinkedin },
            { "social_twitter", this.SocialTwitter },
            { "social_instagram", this.SocialInstagram },
            { "social_tiktok", this.SocialTiktok },
        };
    }
}
