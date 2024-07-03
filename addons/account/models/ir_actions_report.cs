csharp
public partial class ActionsReport
{
    public Dictionary<int, Stream> RenderQwebPdfPrepareStreams(string reportRef, Dictionary<string, object> data, List<int> resIds = null)
    {
        if (GetReport(reportRef).ReportName != "account.report_original_vendor_bill")
        {
            return base.RenderQwebPdfPrepareStreams(reportRef, data, resIds);
        }

        var invoices = Env.Get<Account.Move>().Browse(resIds);
        var originalAttachments = invoices.MessageMainAttachmentId;
        if (originalAttachments.IsNullOrEmpty())
        {
            throw new UserError("No original purchase document could be found for any of the selected purchase documents.");
        }

        var collectedStreams = new Dictionary<int, Dictionary<string, object>>();
        foreach (var invoice in invoices)
        {
            var attachment = invoice.MessageMainAttachmentId;
            if (attachment != null)
            {
                var stream = PdfHelper.ToPdfStream(attachment);
                if (stream != null)
                {
                    var record = Env.Get(attachment.ResModel).Browse(attachment.ResId);
                    try
                    {
                        stream = PdfHelper.AddBanner(stream, record.Name, logo: true);
                    }
                    catch (Exception)
                    {
                        record.MessageLog("There was an error when trying to add the banner to the original PDF.\nPlease make sure the source file is valid.");
                    }
                    collectedStreams[invoice.Id] = new Dictionary<string, object>
                    {
                        ["stream"] = stream,
                        ["attachment"] = attachment
                    };
                }
            }
        }
        return collectedStreams;
    }

    public bool IsInvoiceReport(string reportRef)
    {
        return GetReport(reportRef).IsInvoiceReport;
    }

    public Tuple<object, string> PreRenderQwebPdf(string reportRef, List<int> resIds = null, Dictionary<string, object> data = null)
    {
        if (IsInvoiceReport(reportRef))
        {
            var invoices = Env.Get<Account.Move>().Browse(resIds);
            if (Env.Get<Ir.ConfigParameter>().Sudo().GetParam("account.display_name_in_footer"))
            {
                data = data ?? new Dictionary<string, object>();
                data["display_name_in_footer"] = true;
            }
            if (invoices.Any(x => x.MoveType == "entry"))
            {
                throw new UserError("Only invoices could be printed.");
            }
        }

        var result = base.PreRenderQwebPdf(reportRef, resIds, data);
        var content = result.Item1;
        var reportType = result.Item2;

        if (IsInvoiceReport(reportRef))
        {
            if (reportType == "html")
            {
                var report = GetReport(reportRef);
                var prepareHtmlResult = PrepareHtml(content, reportModel: report.Model);
                var bodies = prepareHtmlResult.Item1;
                var preparedResIds = prepareHtmlResult.Item2;
                return Tuple.Create(
                    preparedResIds.Zip(bodies, (resId, body) => new KeyValuePair<int, byte[]>(resId, Encoding.UTF8.GetBytes(body.ToString())))
                        .ToDictionary(x => x.Key, x => x.Value),
                    "html"
                );
            }
            else if (reportType == "pdf")
            {
                var pdfDict = ((Dictionary<int, Dictionary<string, object>>)content).ToDictionary(
                    kvp => kvp.Key,
                    kvp => ((MemoryStream)kvp.Value["stream"]).ToArray()
                );
                foreach (var stream in ((Dictionary<int, Dictionary<string, object>>)content).Values)
                {
                    ((MemoryStream)stream["stream"]).Close();
                }
                return Tuple.Create((object)pdfDict, "pdf");
            }
        }
        return result;
    }

    public Dictionary<string, object> GetRenderingContext(IrActionsReport report, List<int> docids, Dictionary<string, object> data)
    {
        var result = base.GetRenderingContext(report, docids, data);
        if (Env.Context.ContainsKey("proforma_invoice"))
        {
            result["proforma"] = true;
        }
        return result;
    }
}
