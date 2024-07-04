csharp
public partial class AccountEdiFormat
{
    public bool L10nInEdiEwaybillBaseIrnOrDirect(Account.Move move)
    {
        if (move.MoveType == "out_refund" || move.DebitOriginId != null)
        {
            return "direct";
        }
        var einvoiceInEdiFormat = move.JournalId.EdiFormatIds.FirstOrDefault(f => f.Code == "in_einvoice_1_03");
        return einvoiceInEdiFormat != null && einvoiceInEdiFormat.GetMoveApplicability(move) ? "irn" : "direct";
    }

    public bool IsCompatibleWithJournal(Account.Journal journal)
    {
        if (this.Code == "in_ewaybill_1_03")
        {
            return false;
        }
        return base.IsCompatibleWithJournal(journal);
    }

    public bool IsEnabledByDefaultOnJournal(Account.Journal journal)
    {
        if (this.Code == "in_ewaybill_1_03")
        {
            return false;
        }
        return base.IsEnabledByDefaultOnJournal(journal);
    }

    public Dictionary<string, object> GetMoveApplicability(Account.Move invoice)
    {
        if (this.Code != "in_ewaybill_1_03")
        {
            return base.GetMoveApplicability(invoice);
        }

        if (invoice.IsInvoice() && invoice.CountryCode == "IN")
        {
            var res = new Dictionary<string, object>
            {
                { "post", L10nInEdiEwaybillPostInvoiceEdi },
                { "cancel", L10nInEdiEwaybillCancelInvoice },
                { "edi_content", L10nInEdiEwaybillJsonInvoiceContent }
            };
            var @base = L10nInEdiEwaybillBaseIrnOrDirect(invoice);
            if (@base == "irn")
            {
                res["post"] = L10nInEdiEwaybillIrnPostInvoiceEdi;
                res["edi_content"] = L10nInEdiEwaybillIrnJsonInvoiceContent;
            }
            return res;
        }
        return null;
    }

    // Add other methods here...
}
