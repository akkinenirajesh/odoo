csharp
public partial class WebsitePage {
    public void ComputeIsHomepage() {
        var website = Env.Ref<Website.Website>("website.default_website");
        this.IsHomepage = this.Url == (website.HomepageUrl ?? (this.WebsiteId == website && "/"));
    }

    public void SetIsHomepage() {
        var website = Env.Ref<Website.Website>("website.default_website");
        if (this.IsHomepage) {
            if (website.HomepageUrl != this.Url) {
                website.HomepageUrl = this.Url;
            }
        } else {
            if (website.HomepageUrl == this.Url) {
                website.HomepageUrl = "";
            }
        }
    }

    public void ComputeVisible() {
        this.IsVisible = this.WebsitePublished && (
            this.DatePublish == null || this.DatePublish < DateTime.Now
        );
    }

    public void ComputeWebsiteMenu() {
        this.IsInMenu = this.MenuIds.Any();
    }

    public void InverseWebsiteMenu() {
        if (this.IsInMenu) {
            if (!this.MenuIds.Any()) {
                Env.Create<WebsiteMenu.Menu>(new {
                    Name = this.Name,
                    Url = this.Url,
                    PageId = this.Id,
                    ParentId = this.WebsiteId.MenuId.Id,
                    WebsiteId = this.WebsiteId.Id
                });
            }
        } else if (this.MenuIds.Any()) {
            this.MenuIds.Unlink();
        }
    }
}
