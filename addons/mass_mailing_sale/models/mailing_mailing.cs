csharp
public partial class MassMailing
{
    public int SaleQuotationCount { get; set; }
    public int SaleInvoicedAmount { get; set; }

    public void ComputeSaleQuotationCount()
    {
        var quotationData = Env.Get("sale.order")._ReadGroup(
            new[] { new[] { "SourceId", "in", this.SourceId.Ids }, new[] { "OrderLine", "!=", null } },
            new[] { "SourceId" }, new[] { "__count" });

        var mappedData = quotationData.ToDictionary(x => x["SourceId"], x => x["__count"]);
        SaleQuotationCount = mappedData.GetValueOrDefault(SourceId.Id, 0);
    }

    public void ComputeSaleInvoicedAmount()
    {
        var domain = new[] {
            new[] { "SourceId", "in", this.SourceId.Ids },
            new[] { "State", "not in", new[] { "draft", "cancel" } }
        };
        var movesData = Env.Get("account.move")._ReadGroup(
            domain, new[] { "SourceId" }, new[] { "AmountUntaxedSigned:sum" });
        var mappedData = movesData.ToDictionary(x => x["SourceId"], x => x["AmountUntaxedSigned"]);
        SaleInvoicedAmount = mappedData.GetValueOrDefault(SourceId.Id, 0);
    }

    public Dictionary<string, object> ActionRedirectToQuotations()
    {
        return new Dictionary<string, object>() {
            { "context", new Dictionary<string, object>() {
                { "create", false },
                { "search_default_group_by_date_day", true },
                { "sale_report_view_hide_date", true }
            } },
            { "domain", new[] { new[] { "SourceId", "=", this.SourceId.Id } } },
            { "help", $"<p class=\"o_view_nocontent_smiling_face\">{__("No Quotations yet!")}</p><p>{__("Quotations will appear here once your customers add products to their Carts or when your sales reps assign this mailing.")}</p>" },
            { "name", _("Sales Analysis") },
            { "res_model", "sale.report" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "tree,pivot,graph,form" }
        };
    }

    public Dictionary<string, object> ActionRedirectToInvoiced()
    {
        var domain = new[] {
            new[] { "SourceId", "=", this.SourceId.Id },
            new[] { "State", "not in", new[] { "draft", "cancel" } }
        };
        var moves = Env.Get("account.move").Search(domain);
        return new Dictionary<string, object>() {
            { "context", new Dictionary<string, object>() {
                { "create", false },
                { "edit", false },
                { "view_no_maturity", true },
                { "search_default_group_by_invoice_date_week", true },
                { "invoice_report_view_hide_invoice_date", true }
            } },
            { "domain", new[] { new[] { "MoveId", "in", moves.Ids } } },
            { "help", $"<p class=\"o_view_nocontent_smiling_face\">{__("No Revenues yet!")}</p><p>{__("Revenues will appear here once orders are turned into invoices.")}</p>" },
            { "name", _("Invoices Analysis") },
            { "res_model", "account.invoice.report" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "tree,pivot,graph,form" }
        };
    }

    public Dictionary<string, object> PrepareStatisticsEmailValues()
    {
        var values = base.PrepareStatisticsEmailValues();
        if (this.UserId == null)
        {
            return values;
        }

        var selfWithCompany = this.WithCompany(this.UserId.CompanyId);
        var currency = UserId.CompanyId.CurrencyId;
        var formattedAmount = Env.Tools.FormatDecimalizedAmount(selfWithCompany.SaleInvoicedAmount, currency);

        values["kpi_data"][1]["kpi_col2"] = new Dictionary<string, object>()
        {
            { "value", SaleQuotationCount },
            { "col_subtitle", _("QUOTATIONS") }
        };
        values["kpi_data"][1]["kpi_col3"] = new Dictionary<string, object>()
        {
            { "value", formattedAmount },
            { "col_subtitle", _("INVOICED") }
        };
        values["kpi_data"][1]["kpi_name"] = "sale";
        return values;
    }
}
