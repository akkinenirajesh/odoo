csharp
public partial class WebsiteSaleSlides.SlideChannel
{
    public decimal ProductSaleRevenues { get; set; }
    public Core.Currency CurrencyId { get; set; }

    public WebsiteSaleSlides.SlideChannelEnroll Enroll { get; set; }

    public Product.Product ProductId { get; set; }

    private Product.Product GetDefaultProductId()
    {
        var productCourses = Env.GetModel("Product.Product").Search(
            new[] { new("ServiceTracking", "=", "course") }, limit: 2);
        return productCourses.Count == 1 ? productCourses[0] : null;
    }

    public void ComputeProductSaleRevenues()
    {
        var doneStates = Env.GetModel("Sale.Report")._GetDoneStates();
        var domain = new[]
        {
            new("State", "in", doneStates),
            new("ProductId", "in", ProductId.Id)
        };
        var rgData = Env.GetModel("Sale.Report")
            ._ReadGroup(domain, new[] { "ProductId" }, new[] { "PriceTotal:sum" })
            .ToDictionary(rg => rg["ProductId"], rg => rg["PriceTotal"]);
        ProductSaleRevenues = rgData.GetValueOrDefault(ProductId.Id, 0);
    }

    public void SynchronizeProductPublish()
    {
        if (IsPublished && !ProductId.IsPublished)
        {
            ProductId.Write(new Dictionary<string, object> { { "IsPublished", true } });
        }

        var unpublishedChannelProducts = this.Where(channel => !channel.IsPublished).Select(channel => channel.ProductId);
        var groupData = _ReadGroup(
            new[]
            {
                new("IsPublished", "=", true),
                new("ProductId", "in", unpublishedChannelProducts.Select(product => product.Id))
            },
            new[] { "ProductId" });
        var usedProductIds = groupData.Select(rg => rg["ProductId"]).ToHashSet();
        var productToUnpublish = unpublishedChannelProducts.Where(product => !usedProductIds.Contains(product.Id));
        if (productToUnpublish.Any())
        {
            productToUnpublish.Write(new Dictionary<string, object> { { "IsPublished", false } });
        }
    }

    public void ActionViewSales()
    {
        var action = Env.GetModel("Ir.Actions.Actions")._ForXmlId("website_sale_slides.sale_report_action_slides");
        action["Domain"] = new[] { new("ProductId", "in", ProductId.Id) };
    }

    public List<WebsiteSaleSlides.SlideChannel> FilterAddMembers(List<Core.Partner> targetPartners, bool raiseOnAccess = false)
    {
        var result = base.FilterAddMembers(targetPartners, raiseOnAccess);
        var onPayment = this.Where(channel => channel.Enroll == WebsiteSaleSlides.SlideChannelEnroll.Payment);
        if (onPayment.Any())
        {
            try
            {
                onPayment.CheckAccessRights("write");
                onPayment.CheckAccessRule("write");
            }
            catch (AccessError)
            {
                if (raiseOnAccess)
                {
                    throw new AccessError("You are not allowed to add members to this course. Please contact the course responsible or an administrator.");
                }
            }
            else
            {
                result.AddRange(onPayment);
            }
        }
        return result;
    }
}
