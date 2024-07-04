csharp
public partial class ExpenseSheet
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionSubmitSheet()
    {
        DoSubmit();
    }

    public void ActionApproveExpenseSheets()
    {
        CheckCanApprove();
        ValidateAnalyticDistribution();
        var duplicates = ExpenseLineIds.DuplicateExpenseIds.Where(exp => exp.State == ExpenseState.Approved || exp.State == ExpenseState.Done);
        if (duplicates.Any())
        {
            var action = Env.Ref<IrActionsActWindow>("hr_expense.hr_expense_approve_duplicate_action");
            action.Context["default_sheet_ids"] = new[] { Id };
            action.Context["default_expense_ids"] = duplicates.Select(d => d.Id).ToArray();
            return action;
        }
        DoApprove();
    }

    public void ActionRefuseExpenseSheets()
    {
        CheckCanRefuse();
        return Env.Ref<IrActionsActWindow>("hr_expense.hr_expense_refuse_wizard_action");
    }

    public void ActionSheetMovePost()
    {
        if (!AccountMoveIds.Any())
        {
            DoCreateMoves();
        }
        AccountMoveIds.ActionPost();
    }

    public void ActionResetExpenseSheets()
    {
        if (State != ExpenseSheetState.Draft && State != ExpenseSheetState.Submit)
        {
            CheckCanResetApproval();
        }
        DoReverseMoves();
        DoResetApproval();
        AccountMoveIds.Clear();
    }

    public IrActionsActWindow ActionRegisterPayment()
    {
        return AccountMoveIds.WithContext(new { default_partner_bank_id = Employee.BankAccountIds.Count() <= 1 ? Employee.BankAccountIds.FirstOrDefault()?.Id : null }).ActionRegisterPayment();
    }

    public IrActionsActWindow ActionOpenExpenseView()
    {
        if (NbExpense == 1)
        {
            return new IrActionsActWindow
            {
                Type = "ir.actions.act_window",
                ViewMode = "form",
                ResModel = "HR.Expense",
                ResId = ExpenseLineIds.First().Id,
            };
        }
        return new IrActionsActWindow
        {
            Name = "Expenses",
            Type = "ir.actions.act_window",
            ViewMode = "list,form",
            Views = new[] { new[] { false, "list" }, new[] { false, "form" } },
            ResModel = "HR.Expense",
            Domain = new[] { new[] { "id", "in", ExpenseLineIds.Select(e => e.Id).ToArray() } },
        };
    }

    public IrActionsActWindow ActionOpenAccountMoves()
    {
        string resModel;
        IEnumerable<int> recordIds;
        if (PaymentMode == ExpensePaymentMode.OwnAccount)
        {
            resModel = "Account.Move";
            recordIds = AccountMoveIds.Select(m => m.Id);
        }
        else
        {
            resModel = "Account.Payment";
            recordIds = AccountMoveIds.SelectMany(m => m.PaymentId).Select(p => p.Id);
        }

        var action = new IrActionsActWindow { Type = "ir.actions.act_window", ResModel = resModel };
        if (AccountMoveIds.Count() == 1)
        {
            action.Name = recordIds.First().ToString();
            action.ViewMode = "form";
            action.ResId = recordIds.First();
            action.Views = new[] { new[] { false, "form" } };
        }
        else
        {
            action.Name = "Journal entries";
            action.ViewMode = "list";
            action.Domain = new[] { new[] { "id", "in", recordIds.ToArray() } };
            action.Views = new[] { new[] { false, "list" }, new[] { false, "form" } };
        }
        return action;
    }

    // Other methods would be implemented similarly
}
