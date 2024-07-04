csharp
public partial class ProductTemplateAttributeExclusion {
    public ProductTemplateAttributeExclusion() {}

    public void Create(List<Dictionary<string, object>> valsList) {
        var exclusions = Env.Create(this, valsList);
        exclusions.ProductTmplId.CreateVariantIds();
    }

    public void Unlink() {
        var templates = Env.Get<ProductTemplate>(this.ProductTmplId);
        Env.Delete(this);
        templates.CreateVariantIds();
    }

    public void Write(Dictionary<string, object> values) {
        var templates = Env.Get<ProductTemplate>();
        if (values.ContainsKey("ProductTmplId")) {
            templates = Env.Get<ProductTemplate>(this.ProductTmplId);
        }
        Env.Write(this, values);
        (templates | Env.Get<ProductTemplate>(this.ProductTmplId)).CreateVariantIds();
    }
}
