csharp
public partial class AccountMove {
    public string GetInvoiceReportName() {
        if (Env.Context.Company.AccountFiscalCountryID.Code == "ZM") {
            return "l10n_zm_account.report_invoice_document";
        }
        return this.Name;
    }
}
