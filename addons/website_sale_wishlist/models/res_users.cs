csharp
public partial class WebsiteSaleWishlistResUsers {
    public bool CheckCredentials(string password, Env env) {
        bool result = env.CallMethod("Website.Sale.Wishlist.ResUsers", "_CheckCredentials", new object[] { password, env });
        if (env.HasSession("wishlist_ids")) {
            env.CallMethod("Website.Sale.Wishlist.ProductWishlist", "_CheckWishlistFromSession");
        }
        return result;
    }
}
