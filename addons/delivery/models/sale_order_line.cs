csharp
public partial class SaleOrderLine
{
    public bool IsNotSellableLine()
    {
        return this.IsDelivery || base.IsNotSellableLine();
    }

    public bool CanBeInvoicedAlone()
    {
        return base.CanBeInvoicedAlone() && !this.IsDelivery;
    }

    public void ComputeProductQty()
    {
        if (this.Product == null || this.ProductUom == null || this.ProductUomQty == 0)
        {
            this.ProductQty = 0.0m;
            return;
        }
        this.ProductQty = this.ProductUom.ComputeQuantity(this.ProductUomQty, this.Product.UomId);
    }

    public override void Unlink()
    {
        if (this.IsDelivery && this.Order?.Carrier != null)
        {
            this.Order.Carrier = null;
        }
        base.Unlink();
    }

    public bool IsDelivery()
    {
        return this.IsDelivery;
    }

    public override IEnumerable<SaleOrderLine> CheckLineUnlink()
    {
        var undeletableLines = base.CheckLineUnlink();
        return undeletableLines.Where(line => !line.IsDelivery);
    }

    public override void ComputePricelistItemId()
    {
        if (!this.IsDelivery)
        {
            base.ComputePricelistItemId();
        }
        else
        {
            this.PricelistItemId = null;
        }
    }
}
