csharp
public partial class ProductTemplateAttributeValue
{
    public bool PtavActive { get; set; }
    public string Name { get; set; }
    public ProductAttributeValue ProductAttributeValueId { get; set; }
    public ProductTemplateAttributeLine AttributeLineId { get; set; }
    public double PriceExtra { get; set; }
    public res_currency CurrencyId { get; set; }
    public ICollection<ProductTemplateAttributeExclusion> ExcludeFor { get; set; }
    public ProductTemplate ProductTmplId { get; set; }
    public ProductAttribute AttributeId { get; set; }
    public ICollection<Product> PtavProductVariantIds { get; set; }
    public string HtmlColor { get; set; }
    public bool IsCustom { get; set; }
    public string DisplayType { get; set; }
    public int Color { get; set; }
    public byte[] Image { get; set; }

    private static int RandomColor()
    {
        Random random = new Random();
        return random.Next(1, 12);
    }

    public void CheckValidValues()
    {
        if (PtavActive && !AttributeLineId.ValueIds.Contains(ProductAttributeValueId))
        {
            throw new Exception($"The value {ProductAttributeValueId.DisplayName} is not defined for the attribute {AttributeId.DisplayName} on the product {ProductTmplId.DisplayName}.");
        }
    }

    public void Unlink()
    {
        // Directly remove the values from the variants for lines that had single value (counting also the values that are archived).
        if (AttributeLineId.ProductTemplateValueIds.Count == 1)
        {
            foreach (var ptav in this)
            {
                ptav.PtavProductVariantIds.ForEach(variant => variant.ProductTemplateAttributeValueIds.Remove(ptav));
            }
        }

        // Try to remove the variants before deleting to potentially remove some blocking references.
        PtavProductVariantIds.ForEach(variant => variant.UnlinkOrArchive());

        // Now delete or archive the values.
        var ptavToArchive = Env.Ref<ProductTemplateAttributeValue>();
        foreach (var ptav in this)
        {
            try
            {
                using (var savepoint = Env.Cr.BeginSavepoint())
                {
                    Env.Cr.Execute($"DELETE FROM product_template_attribute_value WHERE id = {ptav.Id}");
                }
            }
            catch (Exception)
            {
                ptavToArchive += ptav;
            }
        }

        ptavToArchive.ForEach(ptav => ptav.PtavActive = false);
    }

    public string ComputeDisplayName()
    {
        return $"{AttributeId.Name}: {Name}";
    }

    public ICollection<ProductTemplateAttributeValue> OnlyActive()
    {
        return this.Where(ptav => ptav.PtavActive).ToList();
    }

    public ICollection<ProductTemplateAttributeValue> WithoutNoVariantAttributes()
    {
        return this.Where(ptav => ptav.AttributeId.CreateVariant != "no_variant").ToList();
    }

    public string Ids2Str()
    {
        return string.Join(",", this.Select(i => i.Id.ToString()).OrderBy(i => i).ToList());
    }

    public string GetCombinationName()
    {
        var ptavs = WithoutNoVariantAttributes();
        ptavs = ptavs.FilterSingleValueLines();
        return string.Join(", ", ptavs.Select(ptav => ptav.Name));
    }

    public ICollection<ProductTemplateAttributeValue> FilterSingleValueLines()
    {
        var onlyActive = this.All(ptav => ptav.PtavActive);
        return this.Where(ptav => !ptav.IsFromSingleValueLine(onlyActive)).ToList();
    }

    public bool IsFromSingleValueLine(bool onlyActive = true)
    {
        var allValues = AttributeLineId.ProductTemplateValueIds;
        if (onlyActive)
        {
            allValues = allValues.OnlyActive();
        }
        return allValues.Count == 1;
    }
}
