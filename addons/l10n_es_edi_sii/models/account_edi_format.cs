csharp
public partial class AccountEdiFormat
{
    public List<AccountMove> LOnEsEdiGetInvoicesTaxDetailsInfo(AccountMove invoice, Func<AccountMoveLine, bool> filterInvlToApply = null)
    {
        // Implementation of _l10n_es_edi_get_invoices_tax_details_info method
        // This method would return a list of tax details for the given invoice
        // You'll need to implement the logic here based on the Python code
    }

    public Dictionary<string, object> LOnEsEdiGetPartnerInfo(ResPartner partner)
    {
        // Implementation of _l10n_es_edi_get_partner_info method
        // This method would return partner information as a dictionary
        // You'll need to implement the logic here based on the Python code
    }

    public List<Dictionary<string, object>> LOnEsEdiGetInvoicesInfo(List<AccountMove> invoices)
    {
        // Implementation of _l10n_es_edi_get_invoices_info method
        // This method would return a list of dictionaries containing invoice information
        // You'll need to implement the logic here based on the Python code
    }

    public Dictionary<string, object> LOnEsEdiWebServiceAeatVals(List<AccountMove> invoices)
    {
        // Implementation of _l10n_es_edi_web_service_aeat_vals method
        // This method would return a dictionary with web service values for AEAT
        // You'll need to implement the logic here based on the Python code
    }

    // Implement other methods similarly...

    public Dictionary<AccountMove, Dictionary<string, object>> LOnEsEdiSiiPostInvoices(List<AccountMove> invoices)
    {
        // Implementation of _l10n_es_edi_sii_post_invoices method
        // This method would post invoices to SII and return the results
        // You'll need to implement the logic here based on the Python code
    }
}

public partial class AccountMove
{
    public bool LOnEsIsDua()
    {
        // Implementation of _l10n_es_is_dua method
        // This method would check if the move is a DUA
        // You'll need to implement the logic here based on the Python code
    }
}
