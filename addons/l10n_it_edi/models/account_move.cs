csharp
public partial class AccountMove
{
    public string ToString()
    {
        return Name;
    }

    public void ComputeLinkedAttachmentId()
    {
        // Implementation for computing L10nItEdiAttachmentId
    }

    public void ComputeL10nItEdiIsSelfInvoice()
    {
        // Implementation for computing L10nItEdiIsSelfInvoice
    }

    public void ComputeL10nItPartnerPa()
    {
        // Implementation for computing L10nItPartnerPa
    }

    public void ActionL10nItEdiSend()
    {
        // Implementation for action_l10n_it_edi_send
    }

    public void ActionCheckL10nItEdi()
    {
        // Implementation for action_check_l10n_it_edi
    }

    public override void ButtonDraft()
    {
        base.ButtonDraft();
        L10nItEdiState = null;
    }

    public bool L10nItEdiReadyForXmlExport()
    {
        return State == "posted"
            && Company.AccountFiscalCountryId?.Code == "IT"
            && Journal.Type == "sale"
            && (L10nItEdiState == null || L10nItEdiState == "rejected");
    }

    public List<Dictionary<string, object>> L10nItEdiGetLineValues(bool reverseChargeRefund = false, bool isDownpayment = false, bool convertToEuros = true)
    {
        // Implementation for _l10n_it_edi_get_line_values
        return new List<Dictionary<string, object>>();
    }

    public List<Dictionary<string, object>> L10nItEdiGetTaxValues(Dictionary<string, object> taxDetails)
    {
        // Implementation for _l10n_it_edi_get_tax_values
        return new List<Dictionary<string, object>>();
    }

    public bool L10nItEdiFilterTaxDetails(AccountMoveLine line, Dictionary<string, object> taxValues)
    {
        // Implementation for _l10n_it_edi_filter_tax_details
        return false;
    }

    public decimal GetL10nItAmountSplitPayment()
    {
        // Implementation for _get_l10n_it_amount_split_payment
        return 0m;
    }

    public Dictionary<string, object> L10nItEdiGetValues(Dictionary<string, object> pdfValues = null)
    {
        // Implementation for _l10n_it_edi_get_values
        return new Dictionary<string, object>();
    }

    // Add other methods as needed...
}
