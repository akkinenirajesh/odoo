csharp
public partial class AccountMove
{
    public string GetNameInvoiceReport()
    {
        if (Env.Company.AccountFiscalCountry?.Code == "MU")
        {
            return "l10n_mu_account.report_invoice_document";
        }
        return base.GetNameInvoiceReport();
    }
}
