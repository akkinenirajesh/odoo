csharp
public partial class Product
{
    public bool IsAddToCartAllowed()
    {
        var res = Env.Call("product.product", "_is_add_to_cart_allowed", this);
        return (bool)res || this.EventTicketIds.Any(x => (bool)Env.Call("event.event.ticket", "WebsitePublished", x));
    }
}

public partial class ProductTemplate
{
    public List<string> GetProductTypesAllowZeroPrice()
    {
        var res = Env.Call("product.template", "_get_product_types_allow_zero_price", this);
        return (List<string>)res;
    }
}
