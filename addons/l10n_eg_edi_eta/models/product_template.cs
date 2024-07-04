csharp
public partial class ProductTemplate
{
    public void _ComputeL10nEgEtaCode()
    {
        this.L10nEgEtaCode = null;
        if (this.ProductVariants.Count == 1)
        {
            this.L10nEgEtaCode = this.ProductVariants[0].L10nEgEtaCode;
        }
    }

    public void _SetL10nEgEtaCode()
    {
        if (this.ProductVariants.Count == 1)
        {
            this.ProductVariants[0].L10nEgEtaCode = this.L10nEgEtaCode;
        }
    }

    public override ProductTemplate Create(Dictionary<string, object> vals)
    {
        var template = base.Create(vals);

        var relatedVals = new Dictionary<string, object>();
        if (vals.ContainsKey("L10nEgEtaCode"))
        {
            relatedVals["L10nEgEtaCode"] = vals["L10nEgEtaCode"];
        }

        if (relatedVals.Count > 0)
        {
            template.Write(relatedVals);
        }

        return template;
    }
}

public partial class ProductProduct
{
    // No additional methods needed for ProductProduct
}
