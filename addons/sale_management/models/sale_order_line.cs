csharp
public partial class SaleOrderLine 
{
    public void ComputeName()
    {
        if (this.ProductId != null && this.OrderId.SaleOrderTemplateId != null && UseTemplateName())
        {
            foreach (var templateLine in this.OrderId.SaleOrderTemplateId.SaleOrderTemplateLineIds)
            {
                if (this.ProductId == templateLine.ProductId)
                {
                    var lang = this.OrderId.PartnerId.Lang;
                    this.Name = templateLine.WithContext(lang).Name + this.WithContext(lang).GetSaleOrderLineMultilineDescriptionVariants();
                    break;
                }
            }
        }
    }

    public bool UseTemplateName()
    {
        return true;
    }

    public void ComputePriceUnit()
    {
        var linesWithoutPriceRecomputation = LinesWithoutPriceRecomputation();
        Env.Call("Sale.SaleOrderLine", "_compute_price_unit", this - linesWithoutPriceRecomputation);
    }

    public SaleOrderLine[] LinesWithoutPriceRecomputation()
    {
        return this.Where(line => line.SaleOrderOptionIds != null).ToArray();
    }

    public bool CanBeEditedOnPortal()
    {
        return this.OrderId.CanBeEditedOnPortal() && (
            this.SaleOrderOptionIds != null ||
            this.ProductId in this.OrderId.SaleOrderOptionIds.ProductId
        );
    }
}
