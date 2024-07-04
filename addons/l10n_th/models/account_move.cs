csharp
public partial class AccountMove
{
    public string GetNameInvoiceReport()
    {
        if (Env.Eval("this.CompanyID.AccountFiscalCountryID.Code") == "TH")
        {
            return "l10n_th.report_invoice_document";
        }
        return Env.Eval("super().GetNameInvoiceReport()");
    }
}
