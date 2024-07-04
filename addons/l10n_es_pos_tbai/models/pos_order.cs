csharp
public partial class PosOrder
{
    public string GetL10nEsPosTbaiQrurl()
    {
        // Ensure this is a single order
        if (this == null)
            throw new InvalidOperationException("This method can only be called on a single POS order.");

        if (this.AccountMove.EdiDocumentIds.Any(d => d.EdiFormatId.Code == "es_tbai"))
        {
            var tbaiDocumentsToSend = this.AccountMove.EdiDocumentIds
                .Where(d => d.EdiFormatId.Code == "es_tbai" && d.State == "to_send")
                .ToList();

            // Process documents web services
            foreach (var doc in tbaiDocumentsToSend)
            {
                doc.ProcessDocumentsWebServices(1);
            }

            return this.AccountMove.GetL10nEsTbaiQr();
        }

        return null;
    }

    public void GeneratePosOrderInvoice()
    {
        var journal = this.IsL10nEsSimplifiedInvoice
            ? this.Config.L10nEsSimplifiedInvoiceJournalId
            : this.Config.InvoiceJournalId;

        if (journal.EdiFormatIds.Any(f => f.Code == "es_tbai"))
        {
            // Call the base implementation with a specific context
            this.Env.WithContext(new Dictionary<string, object> { { "skip_account_edi_cron_trigger", true } })
                .Super().GeneratePosOrderInvoice();
        }
        else
        {
            // Call the base implementation without any special context
            this.Env.Super().GeneratePosOrderInvoice();
        }
    }
}
