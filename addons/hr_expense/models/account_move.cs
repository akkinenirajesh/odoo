csharp
public partial class AccountMove
{
    public Dictionary<string, object> ActionOpenExpenseReport()
    {
        return new Dictionary<string, object>
        {
            ["name"] = this.ExpenseSheetId.Name,
            ["type"] = "ir.actions.act_window",
            ["view_mode"] = "form",
            ["views"] = new List<object> { new List<object> { false, "form" } },
            ["res_model"] = "HR.ExpenseSheet",
            ["res_id"] = this.ExpenseSheetId.Id
        };
    }

    public bool CheckJournalMoveType()
    {
        // Assuming we have a base class or interface that implements this method
        return base.CheckJournalMoveType();
    }

    public string CreationMessage()
    {
        if (this.ExpenseSheetId != null)
        {
            return $"Expense entry created from: {this.ExpenseSheetId.GetHtmlLink()}";
        }
        return base.CreationMessage();
    }

    public void ComputeNeededTerms()
    {
        // Assuming we have a base implementation
        base.ComputeNeededTerms();

        if (this.ExpenseSheetId != null && this.ExpenseSheetId.PaymentMode == "company_account")
        {
            var termLines = this.LineIds.Where(l => l.DisplayType != "payment_term");
            this.NeededTerms = new Dictionary<object, Dictionary<string, object>>
            {
                [new
                {
                    move_id = this.Id,
                    date_maturity = this.ExpenseSheetId.AccountingDate ?? Env.Context.Today()
                }] = new Dictionary<string, object>
                {
                    ["balance"] = -termLines.Sum(l => l.Balance),
                    ["amount_currency"] = -termLines.Sum(l => l.AmountCurrency),
                    ["name"] = "",
                    ["account_id"] = this.ExpenseSheetId.GetExpenseAccountDestination()
                }
            };
        }
    }

    public List<AccountMove> ReverseMoves(List<Dictionary<string, object>> defaultValuesList = null, bool cancel = false)
    {
        var ownExpenseMoves = this.Where(move => move.ExpenseSheetId?.PaymentMode == "own_account");
        foreach (var move in ownExpenseMoves)
        {
            move.ExpenseSheetId = null;
            move.Ref = null;
        }
        return base.ReverseMoves(defaultValuesList, cancel);
    }

    public void MustDeleteAllExpenseEntries()
    {
        if (this.ExpenseSheetId != null && this.ExpenseSheetId.AccountMoveIds.Except(new List<AccountMove> { this }).Any())
        {
            throw new UserException("You cannot delete only some entries linked to an expense report. All entries must be deleted at the same time.");
        }
    }

    public void ButtonCancel()
    {
        base.ButtonCancel();
        var ownExpenseMoves = this.Where(move => move.ExpenseSheetId?.PaymentMode == "own_account");
        foreach (var move in ownExpenseMoves)
        {
            move.ExpenseSheetId = null;
            move.Ref = null;
        }
    }
}
