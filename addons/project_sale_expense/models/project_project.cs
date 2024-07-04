csharp
public partial class Project {
    public object ComputeExpensesProfitabilityItems() {
        var expensesReadGroup = Env.GetModel("hr.expense")._ReadGroup(
            new[] { new { field = "sheet_id.state", operator = "in", value = new[] { "post", "done" } }, new { field = "analytic_distribution", operator = "in", value = this.AnalyticAccountId.Ids } },
            new[] { "sale_order_id", "product_id", "currency_id" },
            new[] { new { field = "id", aggregate = "array_agg" }, new { field = "untaxed_amount_currency", aggregate = "sum" } }
        );
        if (expensesReadGroup == null) {
            return new { };
        }
        var expensesPerSoId = new Dictionary<long, Dictionary<long, object>>();
        var expenseIds = new List<long>();
        var dictAmountPerCurrency = new Dictionary<long, double>();
        var canSeeExpense = Env.User.HasGroup("hr_expense.group_hr_expense_team_approver");
        foreach (var item in expensesReadGroup) {
            var saleOrderId = (long)item["sale_order_id"];
            var productId = (long)item["product_id"];
            var currencyId = (long)item["currency_id"];
            var ids = (List<long>)item["id:array_agg"];
            var untaxedAmountCurrencySum = (double)item["untaxed_amount_currency:sum"];
            expensesPerSoId.AddOrUpdate(saleOrderId, new Dictionary<long, object>() { { productId, ids } }, (key, value) => {
                value.Add(productId, ids);
                return value;
            });
            if (canSeeExpense) {
                expenseIds.AddRange(ids);
            }
            dictAmountPerCurrency.AddOrUpdate(currencyId, untaxedAmountCurrencySum, (key, value) => value + untaxedAmountCurrencySum);
        }
        var amountBilled = 0.0;
        foreach (var item in dictAmountPerCurrency) {
            amountBilled += Env.GetModel("res.currency").Browse(item.Key).Convert(item.Value, this.CurrencyId.Id, this.CompanyId.Id);
        }
        var solReadGroup = Env.GetModel("sale.order.line")._ReadGroup(
            new[] {
                new { field = "order_id", operator = "in", value = expensesPerSoId.Keys },
                new { field = "is_expense", operator = "=", value = true },
                new { field = "state", operator = "=", value = "sale" },
            },
            new[] { "order_id", "product_id", "currency_id" },
            new[] { new { field = "untaxed_amount_to_invoice", aggregate = "sum" }, new { field = "untaxed_amount_invoiced", aggregate = "sum" } }
        );
        var totalAmountExpenseInvoiced = 0.0;
        var totalAmountExpenseToInvoice = 0.0;
        var reinvoiceExpenseIds = new List<long>();
        var dictInvoicesAmountPerCurrency = new Dictionary<long, Dictionary<string, double>>();
        var setCurrencyIds = new HashSet<long> { this.CurrencyId.Id };
        foreach (var item in solReadGroup) {
            var orderId = (long)item["order_id"];
            var productId = (long)item["product_id"];
            var currencyId = (long)item["currency_id"];
            var untaxedAmountToInvoiceSum = (double)item["untaxed_amount_to_invoice:sum"];
            var untaxedAmountInvoicedSum = (double)item["untaxed_amount_invoiced:sum"];
            var expenseDataPerProductId = expensesPerSoId[orderId];
            setCurrencyIds.Add(currencyId);
            if (expenseDataPerProductId.ContainsKey(productId)) {
                dictInvoicesAmountPerCurrency.AddOrUpdate(currencyId, new Dictionary<string, double>() { { "to_invoice", untaxedAmountToInvoiceSum }, { "invoiced", untaxedAmountInvoicedSum } }, (key, value) => {
                    value["to_invoice"] += untaxedAmountToInvoiceSum;
                    value["invoiced"] += untaxedAmountInvoicedSum;
                    return value;
                });
                reinvoiceExpenseIds.AddRange(expenseDataPerProductId[productId] as List<long>);
            }
        }
        foreach (var item in dictInvoicesAmountPerCurrency) {
            totalAmountExpenseToInvoice += Env.GetModel("res.currency").Browse(item.Key).Convert(item.Value["to_invoice"], this.CurrencyId.Id, this.CompanyId.Id);
            totalAmountExpenseInvoiced += Env.GetModel("res.currency").Browse(item.Key).Convert(item.Value["invoiced"], this.CurrencyId.Id, this.CompanyId.Id);
        }
        var sectionId = "expenses";
        var sequence = this._GetProfitabilitySequencePerInvoiceType()[sectionId];
        var expenseData = new Dictionary<string, Dictionary<string, object>>() {
            {
                "costs", new Dictionary<string, object>() {
                    { "id", sectionId },
                    { "sequence", sequence },
                    { "billed", -amountBilled },
                    { "to_bill", 0.0 },
                }
            }
        };
        if (reinvoiceExpenseIds.Count > 0) {
            expenseData.Add("revenues", new Dictionary<string, object>() {
                { "id", sectionId },
                { "sequence", sequence },
                { "invoiced", totalAmountExpenseInvoiced },
                { "to_invoice", totalAmountExpenseToInvoice },
            });
        }
        if (canSeeExpense) {
            if (reinvoiceExpenseIds.Count > 0) {
                expenseData["revenues"]["action"] = new { name = "action_profitability_items", type = "object", args = System.Text.Json.JsonSerializer.Serialize(new { section = sectionId, domain = new { field = "id", operator = "in", value = reinvoiceExpenseIds }, resId = (reinvoiceExpenseIds.Count == 1 ? reinvoiceExpenseIds[0] : (object)null) }) };
            }
            if (expenseIds.Count > 0) {
                expenseData["costs"]["action"] = new { name = "action_profitability_items", type = "object", args = System.Text.Json.JsonSerializer.Serialize(new { section = sectionId, domain = new { field = "id", operator = "in", value = expenseIds }, resId = (expenseIds.Count == 1 ? expenseIds[0] : (object)null) }) };
            }
        }
        return expenseData;
    }
}
