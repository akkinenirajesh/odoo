csharp
public partial class AccountMove
{
    public void ComputeDeliveryDate()
    {
        // EXTENDS 'account'
        base.ComputeDeliveryDate();
        if (InvoiceDate != null && CountryCode == "DE" && DeliveryDate == null)
        {
            DeliveryDate = InvoiceDate;
        }
    }

    public void ComputeShowDeliveryDate()
    {
        // EXTENDS 'account'
        base.ComputeShowDeliveryDate();
        if (CountryCode == "DE")
        {
            ShowDeliveryDate = IsSaleDocument();
        }
    }

    public bool Post(bool soft = true)
    {
        if (CountryCode == "DE" && IsSaleDocument() && DeliveryDate == null)
        {
            DeliveryDate = InvoiceDate;
        }
        return base.Post(soft);
    }

    private bool IsSaleDocument()
    {
        // Implement the logic to determine if it's a sale document
        // This is a placeholder and should be replaced with actual implementation
        return true;
    }
}
