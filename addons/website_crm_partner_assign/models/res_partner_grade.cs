csharp
public partial class WebsiteCrmPartnerAssign.ResPartnerGrade {
    public void ComputeWebsiteUrl() {
        this.WebsiteUrl = $"/partners/grade/{Env.Slug(this)}";
    }
    public bool DefaultIsPublished() {
        return true;
    }
}
