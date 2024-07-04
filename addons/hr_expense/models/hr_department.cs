csharp
public partial class Department
{
    public void _ComputeExpenseSheetsToApprove()
    {
        var expenseSheetData = Env.Get<HR.ExpenseSheet>().ReadGroup(
            new[] { ("Department", "in", new[] { this.Id }), ("State", "=", "submit") },
            new[] { "Department" },
            new[] { "__count" }
        );

        var result = expenseSheetData.ToDictionary(
            item => item.Department.Id,
            item => item.__count
        );

        this.ExpenseSheetsToApproveCount = result.GetValueOrDefault(this.Id, 0);
    }

    public override string ToString()
    {
        // Assuming Department has a Name field
        return Name;
    }
}
