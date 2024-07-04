csharp
public partial class WebsiteEventBoothSale.ProductProduct
{
    public bool IsAddToCartAllowed { get; set; }

    public void ComputeIsAddToCartAllowed()
    {
        this.IsAddToCartAllowed = base.IsAddToCartAllowed || Env.Get<EventBoothCategory>().SearchCount(x => x.ProductId == this.Id) > 0;
    }
}
