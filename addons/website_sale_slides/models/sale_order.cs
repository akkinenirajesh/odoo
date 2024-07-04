C#
public partial class WebsiteSaleSlides.SaleOrder 
{
    public virtual void ActionConfirm()
    {
        var result = base.ActionConfirm();

        var soLines = Env.Search<WebsiteSaleSlides.SaleOrderLine>(x => x.OrderId == this);
        var products = soLines.Select(x => x.ProductId).ToList();
        var relatedChannels = Env.Search<WebsiteSaleSlides.SlideChannel>(x => products.Contains(x.ProductId) && x.Enroll == WebsiteSaleSlides.SlideChannelEnroll.Payment);
        var channelProducts = relatedChannels.Select(x => x.ProductId).ToList();

        var channelsPerSo = new Dictionary<WebsiteSaleSlides.SaleOrder, WebsiteSaleSlides.SlideChannel>();
        foreach (var soLine in soLines)
        {
            if (channelProducts.Contains(soLine.ProductId))
            {
                foreach (var relatedChannel in relatedChannels)
                {
                    if (relatedChannel.ProductId == soLine.ProductId)
                    {
                        if (!channelsPerSo.ContainsKey(soLine.OrderId))
                        {
                            channelsPerSo.Add(soLine.OrderId, relatedChannel);
                        }
                        else
                        {
                            channelsPerSo[soLine.OrderId] |= relatedChannel;
                        }
                    }
                }
            }
        }

        foreach (var channelPair in channelsPerSo)
        {
            channelPair.Value.ActionAddMembers(channelPair.Key.PartnerId);
        }

        return result;
    }

    public virtual (int, string) VerifyUpdatedQuantity(WebsiteSaleSlides.SaleOrderLine orderLine, Product.Product product, float newQty, Dictionary<string, object> kwargs)
    {
        if (product.ServiceTracking == "course" && newQty > 1)
        {
            return (1, "You can only add a course once in your cart.");
        }
        return base.VerifyUpdatedQuantity(orderLine, product, newQty, kwargs);
    }
}
