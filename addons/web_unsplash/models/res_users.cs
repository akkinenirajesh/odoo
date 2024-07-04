csharp
public partial class WebUnsplashResUsers {

    public bool CanManageUnsplashSettings { get; set; }

    public void ComputeCanManageUnsplashSettings() {
        this.CanManageUnsplashSettings = Env.User.HasGroup("base.group_erp_manager") || Env.User.HasGroup("website.group_website_restricted_editor");
    }

}
