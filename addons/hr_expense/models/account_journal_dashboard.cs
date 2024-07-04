csharp
public partial class AccountJournal
{
    public List<object> PrepareExpenseSheetDataDomain()
    {
        return new List<object>
        {
            new List<object> { "JournalId", "in", new List<int> { this.Id } },
            "|",
            new List<object> { "State", "=", "post" },
            "&",
            new List<object> { "State", "=", "done" },
            new List<object> { "PaymentState", "=", "partial" }
        };
    }

    public object GetExpenseToPayQuery()
    {
        return Env.Get<HrExpenseSheet>().WhereCalc(PrepareExpenseSheetDataDomain());
    }

    public void FillSalePurchaseDashboardData(Dictionary<int, Dictionary<string, object>> dashboardData)
    {
        // Assuming there's a base implementation to call
        base.FillSalePurchaseDashboardData(dashboardData);

        var salePurchaseJournals = new List<AccountJournal> { this }.Where(journal => journal.Type == "sale" || journal.Type == "purchase");
        if (!salePurchaseJournals.Any())
        {
            return;
        }

        var fieldList = new List<string>
        {
            "HrExpenseSheet.JournalId",
            "HrExpenseSheet.TotalAmount AS AmountTotalCompany",
            "HrExpenseSheet.CurrencyId AS Currency"
        };

        var query = GetExpenseToPayQuery().Select(fieldList);
        var queryResultsToPay = Env.Cr.Execute(query).GroupByJournal();

        foreach (var journal in salePurchaseJournals)
        {
            var currency = journal.CurrencyId ?? journal.CompanyId.CurrencyId;
            var (numberExpensesToPay, sumExpensesToPay) = CountResultsAndSumAmounts(queryResultsToPay[journal.Id], currency);

            if (dashboardData.ContainsKey(journal.Id))
            {
                dashboardData[journal.Id]["NumberExpensesToPay"] = numberExpensesToPay;
                dashboardData[journal.Id]["SumExpensesToPay"] = currency.Format(sumExpensesToPay);
            }
        }
    }

    public object OpenExpensesAction()
    {
        var action = Env.Get<IrActionsActWindow>().ForXmlId("hr_expense.action_hr_expense_sheet_all_all");
        action.Context = new Dictionary<string, object>
        {
            { "search_default_approved", 1 },
            { "search_default_to_post", 1 },
            { "search_default_journal_id", this.Id },
            { "default_journal_id", this.Id }
        };
        action.ViewMode = "tree,form";
        action.Views = action.Views.Where(v => v.Item2 == "tree" || v.Item2 == "form").ToList();
        action.Domain = PrepareExpenseSheetDataDomain();
        return action;
    }
}
