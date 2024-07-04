csharp
public partial class ProductTemplate
{
    public virtual List<ProductTemplate> _get_product_types_allow_zero_price()
    {
        List<ProductTemplate> result = Env.Call("Product.ProductTemplate", "_get_product_types_allow_zero_price").ToList<ProductTemplate>();
        result.Add(this);
        return result;
    }
}
