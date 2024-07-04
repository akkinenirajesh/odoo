csharp
public partial class Project {

    private object GetExpenseAction(object domain, object expenseIds) {
        if (domain == null && expenseIds == null) {
            return new { };
        }
        var action = Env.Get("ir.actions.actions").ForXmlId("hr_expense.hr_expense_actions_all");
        action.Update(new {
            DisplayName = "Expenses",
            Views = new object[] { new object[] { false, "tree" }, new object[] { false, "form" }, new object[] { false, "kanban" }, new object[] { false, "graph" }, new object[] { false, "pivot" } },
            Context = new { DefaultAnalyticDistribution = new Dictionary<long, int> { { AnalyticAccountId.Id, 100 } } },
            Domain = domain ?? new object[] { new object[] { "id", "in", expenseIds } }
        });
        if ((List<long>)expenseIds).Count == 1) {
            action.Update(new { Views = new object[] { new object[] { false, "form" } }, ResId = ((List<long>)expenseIds)[0] });
        }
        return action;
    }

    private object GetAddPurchaseItemsDomain() {
        return new List<object> {
            Env.Call("super", "_get_add_purchase_items_domain"),
            new object[] { "expense_id", "=", false }
        };
    }

    public object ActionProfitabilityItems(string sectionName, object domain, long? resId) {
        if (sectionName == "expenses") {
            return GetExpenseAction(domain, resId != null ? new List<long> { resId.Value } : null);
        }
        return Env.Call("super", "action_profitability_items", sectionName, domain, resId);
    }

    private object GetProfitabilityLabels() {
        var labels = Env.Call("super", "_get_profitability_labels");
        labels.Update(new { Expenses = "Expenses" });
        return labels;
    }

    private object GetProfitabilitySequencePerInvoiceType() {
        var sequencePerInvoiceType = Env.Call("super", "_get_profitability_sequence_per_invoice_type");
        sequencePerInvoiceType.Update(new { Expenses = 13 });
        return sequencePerInvoiceType;
    }

    private object GetAlreadyIncludedProfitabilityInvoiceLineIds() {
        var moveLineIds = Env.Call("super", "_get_already_included_profitability_invoice_line_ids");
        var query = Env.Get("account.move.line").Search(new List<object> {
            new object[] { "move_id.expense_sheet_id", "!=", false },
            new object[] { "id", "not in", moveLineIds }
        });
        return (List<long>)moveLineIds + (List<long>)query;
    }

    private object GetExpensesProfitabilityItems(bool withAction) {
        if (AnalyticAccountId == null) {
            return new { };
        }
        var canSeeExpense = withAction && Env.User.HasGroup("hr_expense.group_hr_expense_team_approver");
        var expensesReadGroup = Env.Get("hr.expense").ReadGroup(new List<object> {
            new object[] { "sheet_id.state", "in", new List<string> { "post", "done" } },
            new object[] { "analytic_distribution", "in", new List<long> { AnalyticAccountId.Id } }
        }, new List<string> { "currency_id" }, new List<string> { "id:array_agg", "untaxed_amount_currency:sum" });
        if (expensesReadGroup.Count == 0) {
            return new { };
        }
        var expenseIds = new List<long>();
        var amountBilled = 0.0;
        foreach (var item in expensesReadGroup) {
            var currency = Env.Get("res.currency").Browse((long)item["currency_id"]);
            if (canSeeExpense) {
                expenseIds.AddRange((List<long>)item["id:array_agg"]);
            }
            amountBilled += currency.Convert(item["untaxed_amount_currency:sum"], CurrencyId, CompanyId);
        }
        var sectionId = "expenses";
        var expenseProfitabilityItems = new Dictionary<string, object> {
            { "costs", new { Id = sectionId, Sequence = (int)GetProfitabilitySequencePerInvoiceType()[sectionId], Billed = -amountBilled, ToBill = 0.0 } }
        };
        if (canSeeExpense) {
            var args = new List<object> { sectionId, new object[] { "id", "in", expenseIds } };
            if (expenseIds.Count > 0) {
                args.Add(expenseIds);
            }
            var action = new { Name = "action_profitability_items", Type = "object", Args = JsonConvert.SerializeObject(args) };
            expenseProfitabilityItems.Add("action", action);
        }
        return expenseProfitabilityItems;
    }

    private object GetProfitabilityAalDomain() {
        return new List<object> {
            Env.Call("super", "_get_profitability_aal_domain"),
            new List<object> { "|", new object[] { "move_line_id", "=", false }, new object[] { "move_line_id.expense_id", "=", false } }
        };
    }

    private object GetProfitabilityItems(bool withAction) {
        var profitabilityData = Env.Call("super", "_get_profitability_items", withAction);
        var expensesData = GetExpensesProfitabilityItems(withAction);
        if (expensesData.Count > 0) {
            if (profitabilityData.ContainsKey("revenues")) {
                var revenues = (Dictionary<string, object>)profitabilityData["revenues"];
                revenues["data"].Add(expensesData["revenues"]);
                revenues["total"] = new { Invoiced = (double)revenues["total"]["invoiced"] + (double)expensesData["revenues"]["invoiced"], ToInvoice = (double)revenues["total"]["to_invoice"] + (double)expensesData["revenues"]["to_invoice"] };
            }
            var costs = (Dictionary<string, object>)profitabilityData["costs"];
            costs["data"].Add(expensesData["costs"]);
            costs["total"] = new { Billed = (double)costs["total"]["billed"] + (double)expensesData["costs"]["billed"], ToBill = (double)costs["total"]["to_bill"] + (double)expensesData["costs"]["to_bill"] };
        }
        return profitabilityData;
    }
}
