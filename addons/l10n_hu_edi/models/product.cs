csharp
public partial class ProductTemplate
{
    public override string ToString()
    {
        // Whatever logic to compute the string representation of the object
        return Env.Get<Product.ProductTemplate>(this.Id).Name;
    }
}
