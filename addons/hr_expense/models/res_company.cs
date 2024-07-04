csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming the base class has a Name property
        return Name;
    }

    public Account.AccountJournal GetExpenseJournal()
    {
        return ExpenseJournalId;
    }

    public IEnumerable<Account.AccountPaymentMethodLine> GetAllowedPaymentMethods()
    {
        return CompanyExpenseAllowedPaymentMethodLineIds;
    }

    public void SetExpenseJournal(Account.AccountJournal journal)
    {
        if (journal.Type != "purchase")
        {
            throw new ArgumentException("Expense journal must be of type 'purchase'");
        }
        ExpenseJournalId = journal;
    }

    public void AddAllowedPaymentMethod(Account.AccountPaymentMethodLine paymentMethod)
    {
        if (paymentMethod.PaymentType != "outbound" || paymentMethod.JournalId == null)
        {
            throw new ArgumentException("Payment method must be outbound and associated with a journal");
        }
        CompanyExpenseAllowedPaymentMethodLineIds.Add(paymentMethod);
    }
}
