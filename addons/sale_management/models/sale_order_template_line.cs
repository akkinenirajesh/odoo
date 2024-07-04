csharp
public partial class SaleOrderTemplateLine 
{
    public int SaleOrderTemplateId { get; set; }
    public int Sequence { get; set; }
    public int CompanyId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public int ProductUomId { get; set; }
    public int ProductUomCategoryId { get; set; }
    public double ProductUomQty { get; set; }
    public string DisplayType { get; set; }

    public void _ComputeName()
    {
        if (this.ProductId == 0)
        {
            return;
        }
        this.Name = Env.GetModel("Product.Product").CallMethod<string>("GetProductMultilineDescriptionSale", new object[] { this.ProductId });
    }

    public void _ComputeProductUomId()
    {
        if (this.ProductId == 0)
        {
            return;
        }
        this.ProductUomId = Env.GetModel("Product.Product").CallMethod<int>("GetProductUomId", new object[] { this.ProductId });
    }

    public List<object> _ProductDomain()
    {
        return new List<object>
        {
            new object[] { "SaleOk", "=", true }
        };
    }
}
