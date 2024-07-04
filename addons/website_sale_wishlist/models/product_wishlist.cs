C#
public partial class WebsiteSaleWishlist.ProductWishlist {

    public WebsiteSaleWishlist.ProductWishlist Current()
    {
        if (Env.Request == null)
        {
            return this;
        }

        if (Env.Request.Website.IsPublicUser())
        {
            return Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Search(x => Env.Request.Session.Get("wishlist_ids").Contains(x.Id));
        }

        return Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Search(x => x.PartnerId == Env.User.PartnerId.Id && x.WebsiteId == Env.Request.Website.Id).Where(x =>
            x.ProductId.ProductTmplId.WebsitePublished && x.ProductId.ProductTmplId.CanAddToCart()
        );
    }

    public WebsiteSaleWishlist.ProductWishlist AddToWishlist(long pricelistId, long currencyId, long websiteId, decimal price, long productId, long partnerId)
    {
        return Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Create(new WebsiteSaleWishlist.ProductWishlist
        {
            PartnerId = partnerId,
            ProductId = productId,
            CurrencyId = currencyId,
            PricelistId = pricelistId,
            Price = price,
            WebsiteId = websiteId
        });
    }

    public void CheckWishlistFromSession()
    {
        var sessionWishes = Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Search(x => Env.Request.Session.Get("wishlist_ids").Contains(x.Id));
        var partnerWishes = Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Search(x => x.PartnerId == Env.User.PartnerId.Id);
        var partnerProducts = partnerWishes.Select(x => x.ProductId).ToList();
        var duplicatedWishes = sessionWishes.Where(x => partnerProducts.Contains(x.ProductId)).ToList();
        sessionWishes = sessionWishes.Except(duplicatedWishes).ToList();
        duplicatedWishes.ForEach(x => x.Unlink());
        sessionWishes.ForEach(x => x.PartnerId = Env.User.PartnerId.Id);
        Env.Request.Session.Pop("wishlist_ids");
    }

    public void GCSessions(int wishlistWeek)
    {
        Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Search(x => x.CreateDate < DateTime.Now.AddDays(-wishlistWeek * 7) && x.PartnerId == 0).ForEach(x => x.Unlink());
    }

    public bool IsInWishlist()
    {
        return this in Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Current().Select(x => x.ProductId.ProductTmplId).ToList();
    }

}

public partial class Res.Partner {
}

public partial class Product.Template {

    public bool IsInWishlist()
    {
        return this in Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Current().Select(x => x.ProductId.ProductTmplId).ToList();
    }

}

public partial class Product.Product {

    public bool IsInWishlist()
    {
        return this in Env.Ref<WebsiteSaleWishlist.ProductWishlist>("WebsiteSaleWishlist.ProductWishlist").Current().Select(x => x.ProductId).ToList();
    }

}
