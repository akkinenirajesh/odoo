csharp
public partial class Sale_SaleOrderLine {
    public void ComputePurchasePrice() {
        if (this.IsExpense) {
            var expense = Env.GetRecord<Hr.Expense>(this.ExpenseId);
            var productCost = expense.UntaxedAmountCurrency / (expense.Quantity ?? 1.0);
            this.PurchasePrice = this.ConvertToSolCurrency(productCost, expense.CurrencyId);
        }
    }

    private decimal ConvertToSolCurrency(decimal amount, Core.Currency currency) {
        // Implement currency conversion logic here
        return amount; 
    }
}
