csharp
public partial class ProductAttributeValue {
    public int GetDefaultColor() {
        return new Random().Next(1, 12);
    }

    public void ComputeIsUsedOnProducts() {
        this.IsUsedOnProducts = Env.Get("Product.ProductTemplateAttributeLine").Search(new List<object> { new List<object> { "ProductAttributeValue", "=", this.Id } }).Where(x => Env.Get("Product.ProductTemplate").Get(x.ProductTemplate.Id).Active).Any();
    }

    public void ComputeDisplayName() {
        if (!Env.Context.ContainsKey("ShowAttribute") || (bool)Env.Context["ShowAttribute"]) {
            this.DisplayName = $"{Env.Get("Product.ProductAttribute").Get(this.Attribute.Id).Name}: {this.Name}";
        }
    }

    public void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Attribute")) {
            if (this.Attribute != values["Attribute"] && this.IsUsedOnProducts) {
                throw new Exception($"You cannot change the attribute of the value {this.DisplayName} because it is used on the following products: {string.Join(", ", Env.Get("Product.ProductTemplateAttributeLine").Search(new List<object> { new List<object> { "ProductAttributeValue", "=", this.Id } }).Select(x => Env.Get("Product.ProductTemplate").Get(x.ProductTemplate.Id).DisplayName))}");
            }
        }

        if (values.ContainsKey("Sequence") && this.Sequence != (int)values["Sequence"]) {
            // prefetched o2m have to be resequenced
            // (eg. product.template.attribute.line: value_ids)
            Env.FlushAll();
            Env.InvalidateAll();
        }

        base.Write(values);
    }

    public string CheckIsUsedOnProducts() {
        if (this.IsUsedOnProducts) {
            return $"You cannot delete the value {this.DisplayName} because it is used on the following products:\n{string.Join(", ", Env.Get("Product.ProductTemplateAttributeLine").Search(new List<object> { new List<object> { "ProductAttributeValue", "=", this.Id } }).Select(x => Env.Get("Product.ProductTemplate").Get(x.ProductTemplate.Id).DisplayName))}";
        }

        return null;
    }

    public void UnlinkExceptUsedOnProduct() {
        string isUsedOnProducts = CheckIsUsedOnProducts();
        if (isUsedOnProducts != null) {
            throw new Exception(isUsedOnProducts);
        }
    }

    public void Unlink() {
        var pavsToArchive = Env.Get("Product.ProductAttributeValue");
        foreach (var pav in this) {
            var linkedProducts = Env.Get("Product.ProductTemplateAttributeValue").Search(new List<object> { new List<object> { "ProductAttributeValue", "=", pav.Id } }).Select(x => x.PtavProductVariant.Id).ToList();
            var activeLinkedProducts = linkedProducts.Where(x => Env.Get("Product.ProductVariant").Get(x).Active).ToList();
            if (!activeLinkedProducts.Any()) {
                // If product attribute value found on non-active product variants
                // archive PAV instead of deleting
                pavsToArchive |= pav;
            }
        }

        if (pavsToArchive.Any()) {
            pavsToArchive.ActionArchive();
        }

        base.Unlink();
    }

    public List<ProductAttributeValue> WithoutNoVariantAttributes() {
        return this.Where(pav => Env.Get("Product.ProductAttribute").Get(pav.Attribute.Id).CreateVariant != "no_variant").ToList();
    }

    public Dictionary<string, object> ActionOpenProductTemplateAttributeValue() {
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_window" },
            { "name", "Product Variant Values" },
            { "res_model", "Product.ProductTemplateAttributeValue" },
            { "view_mode", "tree" },
            { "domain", new List<object> { new List<object> { "ProductAttributeValue", "=", this.Id } } }
        };
    }
}
