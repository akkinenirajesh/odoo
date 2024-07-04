csharp
public partial class ProductAttributeCustomValue {
    public void ComputeName() {
        this.Name = (this.CustomValue ?? "").Trim();
        if (Env.Get<Product.ProductTemplateAttributeValue>(this.CustomProductTemplateAttributeValueId).DisplayName) {
            this.Name = string.Format("{0}: {1}", Env.Get<Product.ProductTemplateAttributeValue>(this.CustomProductTemplateAttributeValueId).DisplayName, this.Name);
        }
    }
}
