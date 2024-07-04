csharp
public partial class WebsiteCompany
{
    public void ComputeWebsiteId()
    {
        this.WebsiteId = Env.SearchOne<Website>("company_id", this.Id);
    }

    public void ActionOpenWebsiteThemeSelector()
    {
        var action = Env.GetAction("website.theme_install_kanban_action");
        action.Target = "new";
    }

    public void CheckActive()
    {
        if (!this.Active && this.WebsiteId != null)
        {
            throw new Exception($"The company “{this.Name}” cannot be archived because it has a linked website “{this.WebsiteId.Name}”.\nChange that website's company first.");
        }
    }

    public string GoogleMapImg(int zoom = 8, int width = 298, int height = 298)
    {
        var partner = Env.SearchOne<Partner>("id", this.PartnerId);
        return partner != null ? partner.GoogleMapImg(zoom, width, height) : null;
    }

    public string GoogleMapLink(int zoom = 8)
    {
        var partner = Env.SearchOne<Partner>("id", this.PartnerId);
        return partner != null ? partner.GoogleMapLink(zoom) : null;
    }

    public User GetPublicUser()
    {
        var publicUsers = Env.Ref("base.group_public").Users.Where(user => user.Company == this).ToList();
        if (publicUsers.Count > 0)
        {
            return publicUsers[0];
        }
        else
        {
            return Env.Ref("base.public_user").Copy(new {
                Name = $"Public user for {this.Name}",
                Login = $"public-user@company-{this.Id}.com",
                Company = this,
                CompanyIds = new List<long> { this.Id }
            });
        }
    }
}
