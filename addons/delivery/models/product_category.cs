csharp
public partial class ProductCategory
{
    public override void OnDelete()
    {
        var deliveryCategory = Env.Ref<ProductCategory>("Delivery.ProductCategoryDeliveries", raiseIfNotFound: false);
        if (deliveryCategory != null && this.Equals(deliveryCategory))
        {
            throw new UserException("You cannot delete the deliveries product category as it is used on the delivery carriers products.");
        }
        base.OnDelete();
    }
}
