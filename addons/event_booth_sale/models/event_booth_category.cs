csharp
public partial class EventBoothCategory
{
    public Product.Product DefaultProductId()
    {
        return Env.Ref<Product.Product>("Event.ProductProductEventBooth", raiseIfNotFound: false);
    }

    public override string ToString()
    {
        // Implement string representation logic here
        return $"Event Booth Category: {ProductId?.Name}";
    }

    public void ComputeImage1920()
    {
        if (Image1920 == null || Image1920.Length == 0)
        {
            Image1920 = ProductId?.Image1920;
        }
    }

    public void ComputePrice()
    {
        if (ProductId != null && ProductId.ListPrice.HasValue)
        {
            Price = ProductId.ListPrice.Value + ProductId.PriceExtra;
        }
    }

    public void ComputePriceIncl()
    {
        if (ProductId != null && Price.HasValue)
        {
            var taxIds = ProductId.TaxesId;
            var taxes = taxIds.ComputeAll(Price.Value, CurrencyId, 1.0m, ProductId);
            PriceIncl = taxes.TotalIncluded;
        }
        else
        {
            PriceIncl = 0;
        }
    }

    public void ComputePriceReduce()
    {
        var contextualDiscount = ProductId?.GetContextualDiscount() ?? 0;
        PriceReduce = (1.0m - contextualDiscount) * (Price ?? 0);
    }

    public void ComputePriceReduceTaxinc()
    {
        var taxIds = ProductId?.TaxesId;
        if (taxIds != null && PriceReduce.HasValue)
        {
            var taxes = taxIds.ComputeAll(PriceReduce.Value, CurrencyId, 1.0m, ProductId);
            PriceReduceTaxinc = taxes.TotalIncluded;
        }
        else
        {
            PriceReduceTaxinc = 0;
        }
    }

    public void InitColumn(string columnName)
    {
        if (columnName != "ProductId")
        {
            base.InitColumn(columnName);
            return;
        }

        // Fetch void columns
        var boothCategoryIds = Env.Cr.Query<int>("SELECT Id FROM EventBoothCategory WHERE ProductId IS NULL");
        if (!boothCategoryIds.Any())
        {
            return;
        }

        // Update existing columns
        var defaultBoothProduct = DefaultProductId();
        int productId;

        if (defaultBoothProduct != null)
        {
            productId = defaultBoothProduct.Id;
        }
        else
        {
            var newProduct = Env.Create<Product.Product>(new {
                Name = "Generic Event Booth Product",
                CategoryId = Env.Ref<Product.Category>("Event.ProductCategoryEvents").Id,
                ListPrice = 100,
                StandardPrice = 0,
                Type = "service",
                ServiceTracking = "event_booth",
                InvoicePolicy = "order"
            });

            productId = newProduct.Id;

            Env.Create<Core.IrModelData>(new {
                Name = "product_product_event_booth",
                Module = "event_booth_sale",
                Model = "Product.Product",
                ResId = productId
            });
        }

        Env.Cr.Execute($"UPDATE EventBoothCategory SET ProductId = @ProductId WHERE Id IN @Ids",
            new { ProductId = productId, Ids = boothCategoryIds });
    }
}
