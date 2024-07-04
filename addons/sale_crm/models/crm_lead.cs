csharp
public partial class SaleCrm.CrmLead {
    public decimal SaleAmountTotal { get; set; }
    public int QuotationCount { get; set; }
    public int SaleOrderCount { get; set; }
    public List<Core.SaleOrder> OrderIds { get; set; }

    public void ComputeSaleData() {
        var companyCurrency = this.CompanyCurrency ?? Env.Company.CurrencyId;
        var saleOrders = this.OrderIds.Where(o => o.State != "draft" && o.State != "sent" && o.State != "cancel").ToList();
        this.SaleAmountTotal = saleOrders.Sum(o => o.CurrencyId.Convert(o.AmountUntaxed, companyCurrency, o.CompanyId, o.DateOrder ?? DateTime.Now));
        this.QuotationCount = this.OrderIds.Where(o => o.State == "draft" || o.State == "sent").ToList().Count;
        this.SaleOrderCount = saleOrders.Count;
    }

    public IrActionsActions ActionSaleQuotationsNew() {
        if (this.PartnerId == null) {
            return Env.IrActionsActions.GetForXmlId("sale_crm.crm_quotation_partner_action");
        }
        return this.ActionNewQuotation();
    }

    public IrActionsActions ActionNewQuotation() {
        var action = Env.IrActionsActions.GetForXmlId("sale_crm.sale_action_quotations_new");
        action.Context = this.PrepareOpportunityQuotationContext();
        action.Context.SearchDefaultOpportunityId = this.Id;
        return action;
    }

    public IrActionsActions ActionViewSaleQuotation() {
        var action = Env.IrActionsActions.GetForXmlId("sale.action_quotations_with_onboarding");
        action.Context = this.PrepareOpportunityQuotationContext();
        action.Context.SearchDefaultDraft = true;
        action.Domain = new List<List<object>> { new List<object> { "OpportunityId", "=", this.Id }, this.GetActionViewSaleQuotationDomain() };
        var quotations = this.OrderIds.Where(o => o.State == "draft" || o.State == "sent" || o.State == "cancel").ToList();
        if (quotations.Count == 1) {
            action.Views = new List<List<object>> { new List<object> { Env.Ref("sale.view_order_form").Id, "form" } };
            action.ResId = quotations[0].Id;
        }
        return action;
    }

    public IrActionsActions ActionViewSaleOrder() {
        var action = Env.IrActionsActions.GetForXmlId("sale.action_orders");
        action.Context = new Dictionary<string, object> {
            { "search_default_partner_id", this.PartnerId.Id },
            { "default_partner_id", this.PartnerId.Id },
            { "default_opportunity_id", this.Id },
        };
        action.Domain = new List<List<object>> { new List<object> { "OpportunityId", "=", this.Id }, this.GetLeadSaleOrderDomain() };
        var orders = this.OrderIds.Where(o => o.State != "draft" && o.State != "sent" && o.State != "cancel").ToList();
        if (orders.Count == 1) {
            action.Views = new List<List<object>> { new List<object> { Env.Ref("sale.view_order_form").Id, "form" } };
            action.ResId = orders[0].Id;
        }
        return action;
    }

    public List<List<object>> GetActionViewSaleQuotationDomain() {
        return new List<List<object>> { new List<object> { "State", "in", new List<string> { "draft", "sent", "cancel" } } };
    }

    public List<List<object>> GetLeadQuotationDomain() {
        return new List<List<object>> { new List<object> { "State", "in", new List<string> { "draft", "sent" } } };
    }

    public List<List<object>> GetLeadSaleOrderDomain() {
        return new List<List<object>> { new List<object> { "State", "not in", new List<string> { "draft", "sent", "cancel" } } };
    }

    public Dictionary<string, object> PrepareOpportunityQuotationContext() {
        var quotationContext = new Dictionary<string, object> {
            { "default_opportunity_id", this.Id },
            { "default_partner_id", this.PartnerId.Id },
            { "default_campaign_id", this.CampaignId.Id },
            { "default_medium_id", this.MediumId.Id },
            { "default_origin", this.Name },
            { "default_source_id", this.SourceId.Id },
            { "default_company_id", this.CompanyId ?? Env.Company.Id },
            { "default_tag_ids", new List<object> { 6, 0, this.TagIds.Select(t => t.Id).ToList() } },
        };
        if (this.TeamId != null) {
            quotationContext.Add("default_team_id", this.TeamId.Id);
        }
        if (this.UserId != null) {
            quotationContext.Add("default_user_id", this.UserId.Id);
        }
        return quotationContext;
    }
}
