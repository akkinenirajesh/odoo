csharp
public partial class ProductTag {
    public void ComputeProductIds() {
        this.ProductIds = this.ProductTemplates.ProductVariants.Union(this.ProductVariants).ToList();
    }

    public void SearchProductIds(string operator, string operand) {
        if (operator == "!=" || operator == "<>" || operator == "not in" || operator == "not like") {
            return this.ProductTemplates.ProductVariants.Where(x => x.Id != operand).Union(this.ProductVariants.Where(x => x.Id != operand)).ToList();
        }
        return this.ProductTemplates.ProductVariants.Where(x => x.Id == operand).Union(this.ProductVariants.Where(x => x.Id == operand)).ToList();
    }

    public List<Product.Product> ProductIds { get; set; }
    public List<Product.ProductTemplate> ProductTemplates { get; set; }
    public List<Product.Product> ProductVariants { get; set; }
    public string Name { get; set; }
    public int Sequence { get; set; }
    public string Color { get; set; }
}
