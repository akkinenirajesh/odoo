csharp
public partial class AccountMove
{
    public bool ComputeShowDeliveryDate()
    {
        // Base implementation (assumed to be in another partial class)
        bool baseResult = base.ComputeShowDeliveryDate();

        if (CountryCode == "HU")
        {
            return IsSaleDocument();
        }

        return baseResult;
    }

    public void Post(bool soft = true)
    {
        // Base implementation (assumed to be in another partial class)
        base.Post(soft);

        if (CountryCode == "HU" && IsSaleDocument() && !DeliveryDate.HasValue)
        {
            DeliveryDate = InvoiceDate;
        }
    }

    private bool IsSaleDocument()
    {
        // Implementation of is_sale_document() method
        // This is a placeholder and should be implemented based on your specific requirements
        return false;
    }
}
