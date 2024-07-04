csharp
public partial class EventTemplateTicket
{
    public Product.Product DefaultProductId()
    {
        return Env.Ref("Event.ProductProductEvent", raiseIfNotFound: false);
    }

    public void ComputePrice()
    {
        if (ProductId != null && ProductId.ListPrice.HasValue)
        {
            Price = ProductId.ListPrice.Value;
        }
        else if (!Price.HasValue)
        {
            Price = 0;
        }
    }

    public void ComputeDescription()
    {
        if (ProductId != null && !string.IsNullOrEmpty(ProductId.DescriptionSale))
        {
            Description = ProductId.DescriptionSale;
        }
        else if (string.IsNullOrEmpty(Description))
        {
            Description = null;
        }
    }

    public void ComputePriceReduce()
    {
        decimal contextualDiscount = ProductId?.GetContextualDiscount() ?? 0;
        PriceReduce = (1.0m - contextualDiscount) * (Price ?? 0);
    }

    public override string ToString()
    {
        return Name;
    }
}
