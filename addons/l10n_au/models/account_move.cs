csharp
public partial class AccountMove
{
    public string GetNameInvoiceReport()
    {
        if (this.CompanyId?.AccountFiscalCountryId?.Code == "AU")
        {
            return "l10n_au.report_invoice_document";
        }
        return base.GetNameInvoiceReport();
    }
}
