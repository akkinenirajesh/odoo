csharp
public partial class IrActionsReport
{
    public Stream RenderQwebPdfPrepareStreams(string reportRef, object data, List<int> resIds = null)
    {
        var collectedStreams = base.RenderQwebPdfPrepareStreams(reportRef, data, resIds);

        if (collectedStreams != null && resIds != null && resIds.Count == 1 && IsInvoiceReport(reportRef))
        {
            var invoice = Env.Get<AccountMove>().Browse(resIds[0]);
            if (invoice.IsSaleDocument() && invoice.State != "draft")
            {
                var toEmbed = invoice.EdiDocumentIds;
                if (toEmbed != null && toEmbed.Any())
                {
                    var pdfStream = collectedStreams[invoice.Id].Stream;

                    // Read pdf content
                    var pdfContent = pdfStream.ToArray();
                    using var readerBuffer = new MemoryStream(pdfContent);
                    using var reader = new PdfReader(readerBuffer);

                    // Post-process and embed the additional files
                    using var writer = new PdfWriter(new MemoryStream());
                    using var pdf = new PdfDocument(reader, writer);

                    foreach (var ediDocument in toEmbed)
                    {
                        ediDocument.EdiFormatId.PrepareInvoiceReport(pdf, ediDocument);
                    }

                    // Replace the current content
                    pdfStream.Close();
                    var newPdfStream = new MemoryStream();
                    pdf.CopyTo(newPdfStream);
                    collectedStreams[invoice.Id].Stream = newPdfStream;
                }
            }
        }

        return collectedStreams;
    }

    private bool IsInvoiceReport(string reportRef)
    {
        // Implement the logic to determine if it's an invoice report
        return false; // Placeholder
    }
}
