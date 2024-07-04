csharp
public partial class PosOrder
{
    public void ComputeL10nEsSimplifiedInvoiceNumber()
    {
        if (IsL10nEsSimplifiedInvoice)
        {
            L10nEsSimplifiedInvoiceNumber = AccountMove.Name;
        }
        else
        {
            L10nEsSimplifiedInvoiceNumber = null;
        }
    }

    public Dictionary<string, object> PrepareInvoiceVals()
    {
        var res = base.PrepareInvoiceVals();
        if (Config.IsSpanish && IsL10nEsSimplifiedInvoice)
        {
            res["JournalId"] = Config.L10nEsSimplifiedInvoiceJournal.Id;
        }
        return res;
    }

    public string GetInvoiceName()
    {
        return AccountMove.Name;
    }
}
