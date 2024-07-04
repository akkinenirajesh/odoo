csharp
public partial class AccountJournal
{
    public void OnInvoiceReferenceModelChange(string oldValue, string newValue)
    {
        if (oldValue == "Be" && newValue != "Be")
        {
            this.InvoiceReferenceModel = "Odoo";
        }
    }
}
