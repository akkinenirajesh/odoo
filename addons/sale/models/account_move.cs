csharp
public partial class AccountMove {
    public virtual void Unlink() {
        var downpaymentLines = Env.Get<Sale.SaleOrderLine>().Search(l => l.IsDownpayment && l.InvoiceLines.Contains(this.InvoiceLineIds)).ToList();
        var res = base.Unlink();
        if (downpaymentLines.Any()) {
            downpaymentLines.ForEach(l => l.Unlink());
        }
        return res;
    }

    public virtual void ComputeTeamId() {
        if (this.InvoiceUserId.SaleTeamId == null || !this.IsSaleDocument(true)) {
            return;
        }
        this.TeamId = Env.Get<Crm.Team>().GetDefaultTeamId(this.InvoiceUserId.Id, new List<object>() { new Dictionary<string, object>() { { "Company", this.Company.Id } } });
    }

    public virtual void ComputeOriginSoCount() {
        this.SaleOrderCount = this.InvoiceLineIds.SelectMany(l => l.SaleLineIds).Select(l => l.OrderId).Distinct().Count();
    }

    public virtual void ReverseMoves(List<Dictionary<string, object>> defaultValuesList = null, bool cancel = false) {
        if (defaultValuesList == null) {
            defaultValuesList = new List<Dictionary<string, object>>() { new Dictionary<string, object>() { } };
        }
        defaultValuesList.ForEach(d => {
            d["CampaignId"] = this.CampaignId.Id;
            d["MediumId"] = this.MediumId.Id;
            d["SourceId"] = this.SourceId.Id;
        });
        base.ReverseMoves(defaultValuesList, cancel);
    }

    public virtual void ActionPost() {
        var res = base.ActionPost();
        var dpLines = this.InvoiceLineIds.Where(l => l.SaleLineIds.Any(s => s.IsDownpayment && s.DisplayType == null)).ToList();
        dpLines.ForEach(l => l.ComputeName());
        var downpaymentLines = dpLines.Where(l => l.SaleLineIds.Any(s => !s.OrderId.Locked)).ToList();
        var otherSoLines = downpaymentLines.SelectMany(l => l.SaleLineIds).Select(l => l.OrderId).Distinct().SelectMany(o => o.OrderLine).Except(downpaymentLines).ToList();
        var realInvoices = otherSoLines.SelectMany(l => l.InvoiceLines).Select(l => l.MoveId).Distinct().ToHashSet();
        downpaymentLines.ForEach(l => {
            l.PriceUnit = l.InvoiceLines.Where(i => i.MoveId.State == "posted" && i.MoveId.MoveType == "out_invoice" && !realInvoices.Contains(i.MoveId)).Sum(i => i.PriceUnit) - l.InvoiceLines.Where(i => i.MoveId.State == "posted" && i.MoveId.MoveType != "out_invoice" && !realInvoices.Contains(i.MoveId)).Sum(i => i.PriceUnit);
            l.TaxId = l.InvoiceLines.Where(i => i.MoveId.State == "posted" && !realInvoices.Contains(i.MoveId)).SelectMany(i => i.TaxIds).Distinct().ToList();
        });
        return res;
    }

    public virtual void ButtonDraft() {
        var res = base.ButtonDraft();
        this.InvoiceLineIds.Where(l => l.SaleLineIds.Any(s => s.IsDownpayment && s.DisplayType == null)).ToList().ForEach(l => l.ComputeName());
        return res;
    }

    public virtual void ButtonCancel() {
        var res = base.ButtonCancel();
        this.InvoiceLineIds.Where(l => l.SaleLineIds.Any(s => s.IsDownpayment && s.DisplayType == null)).ToList().ForEach(l => l.ComputeName());
        return res;
    }

    public virtual void Post(bool soft = true) {
        var posted = base.Post(soft);
        posted.Where(m => m.IsInvoice()).ToList().ForEach(i => {
            var payments = i.TransactionIds.Select(t => t.PaymentId).Where(p => p.State == "posted").ToList();
            var moveLines = payments.SelectMany(p => p.LineIds).Where(l => l.AccountType == "asset_receivable" || l.AccountType == "liability_payable" && !l.Reconciled).ToList();
            moveLines.ForEach(l => i.JsAssignOutstandingLine(l.Id));
        });
        return posted;
    }

    public virtual void InvoicePaidHook() {
        var todo = new HashSet<(Sale.SaleOrder, string)>();
        this.Where(m => m.IsInvoice()).ToList().ForEach(i => {
            i.InvoiceLineIds.ForEach(l => {
                l.SaleLineIds.ForEach(s => {
                    todo.Add((s.OrderId, i.Name));
                });
            });
        });
        todo.ToList().ForEach(t => {
            t.Item1.MessagePost(string.Format("Invoice {0} paid", t.Item2));
        });
        return base.InvoicePaidHook();
    }

    public virtual void ActionInvoiceReadyToBeSent() {
        var res = base.ActionInvoiceReadyToBeSent();
        var sendInvoiceCron = Env.Get<Ir.Actions.ActWindow>().Search(c => c.Name == "send_invoice_cron").FirstOrDefault();
        if (sendInvoiceCron != null) {
            sendInvoiceCron.Trigger();
        }
        return res;
    }

    public virtual void ActionViewSourceSaleOrders() {
        var sourceOrders = this.InvoiceLineIds.SelectMany(l => l.SaleLineIds).Select(l => l.OrderId).Distinct().ToList();
        var result = Env.Get<Ir.Actions.ActWindow>().Search(a => a.Name == "action_orders").FirstOrDefault();
        if (sourceOrders.Count > 1) {
            result.Domain = new List<object>() { new Dictionary<string, object>() { { "id", "in", sourceOrders.Select(o => o.Id).ToList() } } };
        }
        else if (sourceOrders.Count == 1) {
            result.Views = new List<object>() { new List<object>() { Env.Get<Ir.Actions.ActWindowView>().Search(v => v.Name == "view_order_form").FirstOrDefault().Id, "form" } };
            result.ResId = sourceOrders[0].Id;
        }
        else {
            result.Type = "ir.actions.act_window_close";
        }
        return result;
    }

    public virtual bool IsDownpayment() {
        return this.InvoiceLineIds.SelectMany(l => l.SaleLineIds).Any() && this.InvoiceLineIds.SelectMany(l => l.SaleLineIds).All(l => l.IsDownpayment);
    }

    public virtual decimal GetSaleOrderInvoicedAmount(Sale.SaleOrder order) {
        var orderAmount = 0m;
        this.ForEach(i => {
            var prices = i.InvoiceLineIds.Where(l => l.SaleLineIds.Any(s => s.OrderId == order)).Sum(l => l.PriceTotal);
            orderAmount += i.CurrencyId.Convert(prices * -i.DirectionSign, order.CurrencyId, i.Company, i.Date);
        });
        return orderAmount;
    }

    public virtual decimal GetPartnerCreditWarningExcludeAmount() {
        var excludeAmount = base.GetPartnerCreditWarningExcludeAmount();
        this.InvoiceLineIds.SelectMany(l => l.SaleLineIds).Select(l => l.OrderId).Distinct().ToList().ForEach(o => {
            var orderAmount = Math.Min(this.GetSaleOrderInvoicedAmount(o), o.AmountToInvoice);
            var orderAmountCompany = o.CurrencyId.Convert(Math.Max(orderAmount, 0), this.Company.CurrencyId, this.Company, DateTime.Now);
            excludeAmount += orderAmountCompany;
        });
        return excludeAmount;
    }

    public virtual void ComputePartnerCredit() {
        base.ComputePartnerCredit();
        this.Where(m => m.IsInvoice(true)).ToList().ForEach(m => {
            var saleOrders = m.InvoiceLineIds.SelectMany(l => l.SaleLineIds).Select(l => l.OrderId).Distinct().ToList();
            var amountTotalCurrency = m.CurrencyId.Convert(m.TaxTotals["amount_total"], m.CompanyCurrencyId, m.Company, m.Date);
            var amountToInvoiceCurrency = saleOrders.Sum(s => s.CurrencyId.Convert(s.AmountToInvoice, m.CompanyCurrencyId, m.Company, m.Date));
            m.PartnerCredit += Math.Max(amountTotalCurrency - amountToInvoiceCurrency, 0);
        });
    }

    public virtual bool IsSaleDocument(bool includeReceipts = false) {
        // Method implementation not provided, as it requires logic specific to the ERP system.
        return false;
    }

    public virtual void JsAssignOutstandingLine(long lineId) {
        // Method implementation not provided, as it requires logic specific to the ERP system.
    }

    public virtual void MessagePost(string body) {
        // Method implementation not provided, as it requires logic specific to the ERP system.
    }

    public virtual void Trigger() {
        // Method implementation not provided, as it requires logic specific to the ERP system.
    }

    public virtual void ForEach(Action<AccountMove> action) {
        // Method implementation not provided, as it requires logic specific to the ERP system.
    }

    // This is the partial class where all props defined in xml will be generated.
}
