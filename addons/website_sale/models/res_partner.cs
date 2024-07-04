csharp
public partial class WebsiteSale.ResPartner
{
    public void ComputeLastWebsiteSOId()
    {
        var SaleOrder = Env.GetModel<Sale.SaleOrder>();
        foreach (var partner in this)
        {
            var isPublic = partner.IsPublic;
            var website = IrHttp.GetRequestWebsite();
            if (website != null && !isPublic)
            {
                partner.LastWebsiteSOId = SaleOrder.Search(new[]
                {
                    new SearchCondition("PartnerId", "=", partner.Id),
                    new SearchCondition("PricelistId", "=", partner.PropertyProductPricelist.Id),
                    new SearchCondition("WebsiteId", "=", website.Id),
                    new SearchCondition("State", "=", "draft")
                }, new SearchOrder[] { new SearchOrder("WriteDate", "desc") }, 1);
            }
            else
            {
                partner.LastWebsiteSOId = SaleOrder;
            }
        }
    }

    public void OnchangePropertyProductPricelist()
    {
        var openOrder = Env.GetModel<Sale.SaleOrder>().WithUser("sudo").Search(new[]
        {
            new SearchCondition("PartnerId", "=", this.Id),
            new SearchCondition("PricelistId", "=", this.PropertyProductPricelist.Id),
            new SearchCondition("PricelistId", "!=", this.PropertyProductPricelist.Id),
            new SearchCondition("WebsiteId", "!=", null),
            new SearchCondition("State", "=", "draft")
        }, 1);

        if (openOrder != null)
        {
            throw new WarningException("Open Sale Orders", "This partner has an open cart. Please note that the pricelist will not be updated on that cart. Also, the cart might not be visible for the customer until you update the pricelist of that cart.");
        }
    }

    public bool CanBeEditedByCurrentCustomer(Sale.SaleOrder saleOrder, string mode)
    {
        var childrenPartnerIds = Env.GetModel<ResPartner>().Search(new[]
        {
            new SearchCondition("Id", "child_of", saleOrder.PartnerId.CommercialPartnerId.Id),
            new SearchCondition("Type", "in", new string[] { "invoice", "delivery", "other" })
        });

        if (this == saleOrder.PartnerId || childrenPartnerIds.Contains(this))
        {
            if (mode == "billing")
            {
                return true;
            }
            if (mode == "shipping" && this.Type == "delivery")
            {
                return true;
            }
        }
        return false;
    }
}
