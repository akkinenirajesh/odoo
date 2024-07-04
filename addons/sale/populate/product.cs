csharp
public partial class Sale_ProductProduct
{
    public List<Sale_InvoicePolicy> InvoicePolicy { get; set; }

    public List<Sale_InvoicePolicy> _PopulateGetProductFactories()
    {
        var result = Env.Call("Sale_ProductProduct", "_PopulateGetProductFactories") as List<Sale_InvoicePolicy>;
        result.Add(new Sale_InvoicePolicy { Name = "order", Weight = 5 });
        result.Add(new Sale_InvoicePolicy { Name = "delivery", Weight = 5 });
        return result;
    }
}
