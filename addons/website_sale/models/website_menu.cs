csharp
public partial class WebsiteMenu
{
    public bool ComputeVisible()
    {
        if (this.Url.StartsWith("/shop"))
        {
            this.IsVisible = Env.Ref<Website.Website>(this.WebsiteID).HasEcommerceAccess();
        }

        return true;
    }
}
