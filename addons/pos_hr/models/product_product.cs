csharp
public partial class ProductProduct
{
    public List<ProductTag> AllProductTagIds { get; set; }

    public virtual List<ProductTag> LoadPosDataFields(ProductConfiguration configId)
    {
        var result = Env.Call("product.product", "_load_pos_data_fields", configId);
        result.Add(this.AllProductTagIds);
        return result;
    }
}
