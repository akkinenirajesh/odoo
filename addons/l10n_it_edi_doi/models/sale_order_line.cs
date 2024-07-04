csharp
public partial class SaleOrderLine
{
    public void ComputeQtyInvoicedPosted()
    {
        decimal qtyInvoicedPosted = 0.0m;
        foreach (var invoiceLine in GetInvoiceLines())
        {
            if (invoiceLine.Move.State == "posted" || invoiceLine.Move.PaymentState == "invoicing_legacy")
            {
                decimal qtyUnsigned = invoiceLine.ProductUom.ComputeQuantity(invoiceLine.Quantity, this.ProductUom);
                decimal qtySigned = qtyUnsigned * -invoiceLine.Move.DirectionSign;
                qtyInvoicedPosted += qtySigned;
            }
        }
        this.QtyInvoicedPosted = qtyInvoicedPosted;
    }

    private IEnumerable<AccountMoveLine> GetInvoiceLines()
    {
        // Implementation to get invoice lines
        // This would typically involve querying the database through Env
        return Env.Query<AccountMoveLine>()
            .Where(line => line.SaleOrderLine == this)
            .ToList();
    }
}
