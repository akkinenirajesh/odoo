csharp
public partial class AccountMoveLine {
    public virtual List<Sale.Expense> ExpenseId { get; set; }

    public virtual List<Sale.SaleOrderLine> _salePrepareSaleLineValues(Sale.SaleOrder order, decimal price) {
        var res = Env.Call("super", "_salePrepareSaleLineValues", this, order, price);
        if (ExpenseId != null) {
            res.ForEach(x => x.ExpenseId = this.ExpenseId);
        }
        return res;
    }
}
