csharp
public partial class SaleProject.ProjectTask 
{
    public virtual void ComputeSaleOrderId()
    {
        if (!this.AllowBillable)
        {
            this.SaleOrderId = null;
            return;
        }

        Sale.SaleOrder saleOrderId = this.SaleOrderId ?? Env.Ref<Sale.SaleOrder>("sale.order");
        if (this.SaleLineId != null)
        {
            saleOrderId = this.SaleLineId.Order.GetAs<Sale.SaleOrder>();
        }
        else if (this.ProjectId.SaleOrderId != null)
        {
            saleOrderId = this.ProjectId.SaleOrderId.GetAs<Sale.SaleOrder>();
            if (this.PartnerId != null)
            {
                if (this.PartnerId != saleOrderId.PartnerId && this.PartnerId != saleOrderId.PartnerShippingId)
                {
                    saleOrderId = null;
                }
            }
        }
        if (saleOrderId != null && this.PartnerId == null)
        {
            this.PartnerId = saleOrderId.PartnerId;
        }
        this.SaleOrderId = saleOrderId;
    }

    public virtual void InversePartnerId()
    {
        if (this.SaleOrderId != null && this.PartnerId != null)
        {
            if (this.PartnerId != this.SaleOrderId.PartnerId && this.PartnerId != this.SaleOrderId.PartnerShippingId)
            {
                this.SaleOrderId = null;
                this.SaleLineId = null;
            }
        }
    }

    public virtual void ComputeSaleLine()
    {
        if (!this.AllowBillable && !this.Parent.AllowBillable)
        {
            this.SaleLineId = null;
            return;
        }

        Sale.SaleOrderLine saleLine = this.SaleLineId;
        if (saleLine == null)
        {
            if (this.Parent.SaleLineId != null && this.Parent.PartnerId == this.PartnerId)
            {
                saleLine = this.Parent.SaleLineId;
            }
            else if (this.ProjectId.SaleLineId != null && this.ProjectId.PartnerId == this.PartnerId)
            {
                saleLine = this.ProjectId.SaleLineId;
            }

            this.SaleLineId = saleLine ?? this.MilestoneId.SaleLineId;
        }
    }

    public virtual void ComputeDisplaySaleOrderButton()
    {
        this.DisplaySaleOrderButton = false;
        if (this.SaleOrderId == null)
        {
            return;
        }

        try
        {
            var saleOrders = Env.Ref<Sale.SaleOrder>("sale.order").Search(x => x.Id.IsIn(this.SaleOrderId.Ids));
            this.DisplaySaleOrderButton = saleOrders.Contains(this.SaleOrderId);
        }
        catch (AccessError)
        {
            this.DisplaySaleOrderButton = false;
        }
    }

    public virtual void ComputeTaskToInvoice()
    {
        this.TaskToInvoice = false;
        if (this.SaleOrderId != null)
        {
            this.TaskToInvoice = this.SaleOrderId.InvoiceStatus != "no" && this.SaleOrderId.InvoiceStatus != "invoiced";
        }
    }

    public virtual Domain SearchTaskToInvoice(string operator, bool value)
    {
        if ((operator == "=" && value) || (operator != "=" && !value))
        {
            return new Domain("SaleOrderId", DomainOperator.InSelect, "SELECT so.id FROM Sale.SaleOrder so WHERE so.InvoiceStatus != 'no' AND so.InvoiceStatus != 'invoiced'");
        }

        return new Domain("SaleOrderId", DomainOperator.NotInSelect, "SELECT so.id FROM Sale.SaleOrder so WHERE so.InvoiceStatus != 'no' AND so.InvoiceStatus != 'invoiced'");
    }

    public virtual void OnChangeSaleLineId()
    {
        if (this.PartnerId == null && this.SaleLineId != null)
        {
            this.PartnerId = this.SaleLineId.OrderPartnerId;
        }
    }

    public virtual Domain GetSaleLineServiceDomain()
    {
        return new Domain("['|', ('order_partner_id.commercial_partner_id.id', 'parent_of', partner_id if partner_id != null else []), ('order_partner_id', '=?', partner_id)]");
    }

    public virtual Domain GetProjectsToMakeBillableDomain(Domain additionalDomain = null)
    {
        return new Domain(
            DomainOperator.And, 
            new Domain(
                DomainOperator.Or,
                new Domain(
                    DomainOperator.And,
                    new Domain("PartnerId", DomainOperator.NotEqual, null),
                    new Domain("AllowBillable", DomainOperator.Equal, false),
                    new Domain("ProjectId", DomainOperator.NotEqual, null)
                ), 
                additionalDomain ?? new Domain()
            )
        );
    }

    public virtual Sale.SaleOrder GetActionViewSoIds()
    {
        return this.SaleOrderId;
    }

    public virtual ActionWindow ActionViewSo()
    {
        Sale.SaleOrder soIds = GetActionViewSoIds();
        ActionWindow actionWindow = new ActionWindow()
        {
            Type = "ir.actions.act_window",
            ResModel = "Sale.SaleOrder",
            Name = "Sales Order",
            Views = new List<List<object>>()
            {
                new List<object>() { false, "tree" },
                new List<object>() { false, "kanban" },
                new List<object>() { false, "form" }
            },
            Context = new Dictionary<string, object>()
            {
                { "create", false },
                { "show_sale", true },
            },
            Domain = new Domain("id", DomainOperator.In, soIds.Ids)
        };

        if (soIds.Count == 1)
        {
            actionWindow.Views = new List<List<object>>()
            {
                new List<object>() { false, "form" }
            };
            actionWindow.ResId = soIds[0].Id;
        }
        return actionWindow;
    }

    public virtual ActionUrl ActionProjectSharingViewSo()
    {
        if (!this.DisplaySaleOrderButton)
        {
            return new ActionUrl();
        }

        return new ActionUrl()
        {
            Name = "Portal Sale Order",
            Type = "ir.actions.act_url",
            Url = this.SaleOrderId.AccessUrl
        };
    }

    public virtual ResPartner.Partner RatingGetPartner()
    {
        ResPartner.Partner partner = this.PartnerId ?? this.SaleLineId.Order.PartnerId;
        return partner ?? base.RatingGetPartner();
    }

    public virtual void GroupExpandSalesOrder(List<Sale.SaleOrder> salesOrders, Domain domain)
    {
        // ...
    }
}
