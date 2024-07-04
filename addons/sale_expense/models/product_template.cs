csharp
public partial class SaleExpense.ProductTemplate
{
    public void ComputeExpensePolicyTooltip()
    {
        if (!this.CanExpense || this.ExpensePolicy == "no")
        {
            this.ExpensePolicyTooltip = null;
        }
        else if (this.ExpensePolicy == "cost")
        {
            this.ExpensePolicyTooltip = Env.Translate("Expenses will be added to the Sales Order at their actual cost when posted.");
        }
        else if (this.ExpensePolicy == "salesPrice")
        {
            this.ExpensePolicyTooltip = Env.Translate("Expenses will be added to the Sales Order at their sales price (product price, pricelist, etc.) when posted.");
        }
    }

    public void ComputeVisibleExpensePolicy()
    {
        if (!this.CanExpense)
        {
            this.VisibleExpensePolicy = false;
            return;
        }

        this.VisibleExpensePolicy = Env.User.IsInGroup("hr_expense.group_hr_expense_user");
    }

    public void ComputeExpensePolicy()
    {
        if (!this.CanExpense)
        {
            this.ExpensePolicy = "no";
        }
    }
}
