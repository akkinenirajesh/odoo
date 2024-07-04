csharp
public partial class ProductTemplate
{
    public override string ToString()
    {
        // Assuming there's a Name field in the ProductTemplate model
        return Name;
    }

    public void OnServiceTrackingChange()
    {
        if (ServiceTracking == Product.ServiceTracking.Event)
        {
            // Implement any specific logic for when ServiceTracking is set to Event
        }
        else
        {
            // Implement logic for other ServiceTracking options
        }
    }
}
