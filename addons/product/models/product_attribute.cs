C#
public partial class ProductAttribute
{
    public virtual void ComputeNumberRelatedProducts()
    {
        var res = this.Env.Get<ProductTemplateAttributeLine>()._ReadGroup(
            new List<object> { this.Id },
            new List<string> { "AttributeId" },
            new List<string> { "__count" });

        this.NumberRelatedProducts = (int)res.FirstOrDefault()?.Values.FirstOrDefault();
    }

    public virtual void ComputeProducts()
    {
        this.ProductTmplIds = this.AttributeLineIds.Select(x => x.ProductTmplId).ToList();
    }

    public virtual void OnChangeDisplayType()
    {
        if (this.DisplayType == "Multi" && this.NumberRelatedProducts == 0)
        {
            this.CreateVariant = "NoVariant";
        }
    }

    public virtual void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("CreateVariant"))
        {
            if ((string)vals["CreateVariant"] != this.CreateVariant && this.NumberRelatedProducts > 0)
            {
                throw new Exception(
                    $"You cannot change the Variants Creation Mode of the attribute {this.Name}" +
                    $" because it is used on the following products:\n{string.Join(", ", this.ProductTmplIds.Select(x => x.Name))}");
            }
        }

        if (vals.ContainsKey("Sequence"))
        {
            // prefetched o2m have to be resequenced
            // (eg. product.template: attribute_line_ids)
            this.Env.FlushAll();
            this.Env.InvalidateAll();
        }

        // call base write method
    }

    public virtual void UnlinkExceptUsedOnProduct()
    {
        if (this.NumberRelatedProducts > 0)
        {
            throw new Exception(
                $"You cannot delete the attribute {this.Name} because it is used on the" +
                $" following products:\n{string.Join(", ", this.ProductTmplIds.Select(x => x.Name))}");
        }
    }

    public virtual void ActionOpenProductTemplateAttributeLines()
    {
        // TODO: implement this method
    }

    public virtual List<ProductAttribute> WithoutNoVariantAttributes()
    {
        return this.Where(pa => pa.CreateVariant != "NoVariant").ToList();
    }
}
