csharp
public partial class SaleOrder
{
    public int ExpenseCount { get; set; }

    public void ComputeExpenseCount()
    {
        var expenseData = Env.Get("hr.expense").ReadGroup(new[] { new FieldFilter("SaleOrderId", "in", this.Id) }, new[] { "SaleOrderId" }, new[] { "__count" });
        var mappedData = expenseData.Select((expense) => new { SaleOrderId = expense[0].Id, Count = expense[0].Count }).ToDictionary(x => x.SaleOrderId, x => x.Count);
        ExpenseCount = mappedData.GetValueOrDefault(this.Id, 0);
    }

    public IEnumerable<hr.expense> ExpenseIds { get; set; }
}
