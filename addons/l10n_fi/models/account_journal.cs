csharp
public partial class AccountJournal
{
    public void OnInvoiceReferenceModelChanged(Account.InvoiceReferenceModel oldValue, Account.InvoiceReferenceModel newValue)
    {
        if (oldValue == Account.InvoiceReferenceModel.Fi || oldValue == Account.InvoiceReferenceModel.FiRf)
        {
            this.InvoiceReferenceModel = Account.InvoiceReferenceModel.Odoo;
        }
    }
}
