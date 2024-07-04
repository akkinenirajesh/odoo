csharp
public partial class Expense
{
    public override string ToString()
    {
        return Name;
    }

    public void AttachDocument(int[] attachmentIds)
    {
        // Implementation for attaching documents
    }

    public Dictionary<string, object> GetExpenseDashboard()
    {
        // Implementation for getting expense dashboard data
        return new Dictionary<string, object>();
    }

    public void ActionShowSameReceiptExpenseIds()
    {
        // Implementation for showing expenses with same receipt
    }

    public void ActionViewSheet()
    {
        // Implementation for viewing the expense sheet
    }

    public void ActionSplitWizard()
    {
        // Implementation for splitting the expense
    }

    private (Product.Product Product, float Price, Core.Currency Currency, string Description) ParseExpenseSubject(string expenseDescription, Core.Currency[] currencies)
    {
        // Implementation for parsing expense subject
        return (null, 0, null, "");
    }

    private void SendExpenseSuccessMail(Dictionary<string, object> msgDict)
    {
        // Implementation for sending success mail
    }
}
