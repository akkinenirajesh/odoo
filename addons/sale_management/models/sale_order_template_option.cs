csharp
public partial class SaleOrderTemplateOption 
{
    public void ComputeName()
    {
        if (this.ProductId == null)
        {
            return;
        }
        this.Name = Env.Call("Product.Product", "GetProductMultilineDescriptionSale", this.ProductId);
    }

    public void ComputeUomId()
    {
        this.UomId = this.ProductId.UomId;
    }
}
