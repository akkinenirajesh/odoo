csharp
public partial class AccountJournal
{
    public string ProcessReferenceForSaleOrder(string orderReference)
    {
        if (this.InvoiceReferenceModel == InvoiceReferenceModel.Ch)
        {
            // Converting the sale order name into a unique number. Letters are converted to their base10 value
            string invoiceRef = string.Concat(orderReference.Select(a => char.IsDigit(a) ? a.ToString() : ((int)a).ToString()));
            
            // Compute QRR number
            return Env.Get<AccountMove>().ComputeQrrNumber(invoiceRef);
        }
        
        // Call the base implementation for other cases
        return base.ProcessReferenceForSaleOrder(orderReference);
    }
}
