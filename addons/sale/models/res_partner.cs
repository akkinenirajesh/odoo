csharp
public partial class SaleResPartner
{
    public int SaleOrderCount { get; set; }

    public void ComputeSaleOrderCount()
    {
        var allPartners = Env.Model("ResPartner").Search(new List<object> { new List<object> { "Id", "child_of", this.Id } });
        var saleOrderGroups = Env.Model("Sale.SaleOrder").ReadGroup(
            new List<object> {
                new List<object> { this.GetSaleOrderDomainCount() },
                new List<object> { "PartnerId", "in", allPartners.Ids }
            },
            new List<string> { "PartnerId" },
            new List<string> { "__count" });
        foreach (var group in saleOrderGroups)
        {
            var partnerId = group.Values["PartnerId"];
            var count = group.Values["__count"];
            var partner = Env.Model("ResPartner").Browse(partnerId);
            while (partner != null)
            {
                if (partner.Id == this.Id)
                {
                    partner.SaleOrderCount += count;
                }
                partner = partner.ParentId;
            }
        }
    }

    public bool HasOrder(List<object> partnerDomain)
    {
        var saleOrder = Env.Model("Sale.SaleOrder").Search(
            new List<object> {
                new List<object> { partnerDomain },
                new List<object> { "State", "in", new List<string> { "sent", "sale" } }
            },
            1);
        return saleOrder.Count > 0;
    }

    public bool CanEditName()
    {
        return base.CanEditName() && !HasOrder(new List<object> {
            new List<object> { "PartnerInvoiceId", "=", this.Id },
            new List<object> { "PartnerId", "=", this.Id }
        });
    }

    public bool CanEditVat()
    {
        return base.CanEditVat() && !HasOrder(new List<object> {
            new List<object> { "PartnerId", "child_of", this.CommercialPartnerId.Id }
        });
    }

    public object ActionViewSaleOrder()
    {
        var action = Env.Model("IrActionsActWindow").ForXmlId("Sale.act_res_partner_2_sale_order");
        var allChild = Env.Model("ResPartner").Search(new List<object> { new List<object> { "Id", "child_of", this.Ids } });
        action["Domain"] = new List<object> { "PartnerId", "in", allChild.Ids };
        return action;
    }

    public void ComputeCreditToInvoice()
    {
        base.ComputeCreditToInvoice();
        var company = Env.Company;
        var domain = new List<object> {
            new List<object> { "CompanyId", "=", company.Id },
            new List<object> { "PartnerId", "in", this.Ids },
            new List<object> { "AmountToInvoice", ">", 0 },
            new List<object> { "State", "=", "sale" }
        };
        var group = Env.Model("Sale.SaleOrder").ReadGroup(domain, new List<string> { "PartnerId", "CurrencyId" }, new List<string> { "AmountToInvoice:sum" });
        foreach (var groupItem in group)
        {
            var partnerId = groupItem.Values["PartnerId"];
            var currency = Env.Model("ResCurrency").Browse(groupItem.Values["CurrencyId"]);
            var amountToInvoiceSum = groupItem.Values["AmountToInvoice:sum"];
            var creditCompanyCurrency = currency.Convert(amountToInvoiceSum, company.CurrencyId, company, DateTime.Now);
            var partner = Env.Model("ResPartner").Browse(partnerId);
            partner.CreditToInvoice += creditCompanyCurrency;
        }
    }

    public void Unlink()
    {
        var saleOrders = Env.Model("Sale.SaleOrder").Search(new List<object> {
            new List<object> { "State", "in", new List<string> { "draft", "cancel" } },
            new List<object> { "Or", new List<object> {
                new List<object> { "PartnerId", "in", this.Ids },
                new List<object> { "PartnerInvoiceId", "in", this.Ids },
                new List<object> { "PartnerShippingId", "in", this.Ids }
            } }
        });
        saleOrders.Unlink();
        base.Unlink();
    }

    private List<object> GetSaleOrderDomainCount()
    {
        return new List<object>();
    }
}
