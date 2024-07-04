csharp
public partial class ProductTemplate
{
    public string PrepareServiceTrackingTooltip()
    {
        if (ServiceTracking == ServiceTracking.EventBooth)
        {
            return "Mark the selected Booth as Unavailable.";
        }
        return base.PrepareServiceTrackingTooltip();
    }

    public void OnChangeTypeEventBooth()
    {
        if (ServiceTracking == ServiceTracking.EventBooth)
        {
            InvoicePolicy = InvoicePolicy.Order;
        }
    }
}
