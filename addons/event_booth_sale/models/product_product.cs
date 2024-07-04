csharp
public partial class Product
{
    public void OnChangeServiceTracking()
    {
        if (this.ServiceTracking == "event_booth")
        {
            this.InvoicePolicy = "order";
        }
    }
}
