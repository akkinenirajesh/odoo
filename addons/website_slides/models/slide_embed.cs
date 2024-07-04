csharp
public partial class WebsiteEmbeddedSlide {
    public void ComputeWebsiteName() {
        this.WebsiteName = this.Url ?? Env.Translate("Unknown Website");
    }
}
