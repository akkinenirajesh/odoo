csharp
public partial class ProductTemplate
{
    public string PrepareServiceTrackingTooltip()
    {
        if (ServiceTracking == Product.ServiceTracking.Event)
        {
            return "Create an Attendee for the selected Event.";
        }
        return base.PrepareServiceTrackingTooltip();
    }

    public void OnChangeTypeEvent()
    {
        if (ServiceTracking == Product.ServiceTracking.Event)
        {
            InvoicePolicy = Product.InvoicePolicy.Order;
        }
    }
}
