csharp
public partial class WebsiteResCompany {
    public Dictionary<string, string> GetSocialMediaLinks() {
        Dictionary<string, string> socialMediaLinks = Env.Call("website", "GetSocialMediaLinks").ToObject<Dictionary<string, string>>();
        Website website = Env.Call("website", "GetCurrentWebsite").ToObject<Website>();
        socialMediaLinks.AddOrUpdate("SocialFacebook", website.SocialFacebook);
        socialMediaLinks.AddOrUpdate("SocialLinkedin", website.SocialLinkedin);
        socialMediaLinks.AddOrUpdate("SocialTwitter", website.SocialTwitter);
        socialMediaLinks.AddOrUpdate("SocialInstagram", website.SocialInstagram);
        socialMediaLinks.AddOrUpdate("SocialTiktok", website.SocialTiktok);
        return socialMediaLinks;
    }
}
