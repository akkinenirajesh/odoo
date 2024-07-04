csharp
public partial class SaleOrder {
    public void ConfirmSaleOrder(double sampleRatio)
    {
        // Use Env to access external objects
        // Example:
        // var saleOrderLines = Env.GetModel<SaleOrderLine>();
        // var random = Env.GetRandom(); 
        
        // Implement confirmation logic
    }

    public List<SaleOrder> FilterConfirmableSaleOrders()
    {
        // Implement filtering logic
        return null; // Return list of confirmable orders
    }
}

public partial class SaleOrderLine {
    public SaleOrderLine()
    {
        // Constructor 
    }

    public void Populate(int size)
    {
        // Use Env to access external objects
        // Example:
        // var saleOrder = Env.GetModel<SaleOrder>();
        // var random = Env.GetRandom(); 
        // var product = Env.GetModel<Product.Product>();

        // Implement populate logic
        // Confirm orders based on the sampleRatio
        ConfirmSaleOrder(0.60);
    }
}
