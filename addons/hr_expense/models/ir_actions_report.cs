csharp
public partial class IrActionsReport
{
    public Stream RenderQwebPdfPrepareStreams(string reportRef, Dictionary<string, object> data, List<int> resIds = null)
    {
        var res = base.RenderQwebPdfPrepareStreams(reportRef, data, resIds);
        if (resIds == null || resIds.Count == 0)
        {
            return res;
        }

        var report = Env.GetReport(reportRef);
        if (report.ReportName == "hr_expense.report_expense_sheet")
        {
            var expenseSheets = Env.Get<HrExpenseSheet>().Browse(resIds);
            foreach (var expenseSheet in expenseSheets)
            {
                var streamList = new List<Stream>();
                var stream = res[expenseSheet.Id]["stream"] as Stream;
                streamList.Add(stream);

                var attachments = Env.Get<IrAttachment>().Search(new[]
                {
                    ("ResId", "in", expenseSheet.ExpenseLineIds.Select(e => e.Id).ToList()),
                    ("ResModel", "=", "hr.expense")
                });

                using (var expenseReport = new PdfReader(stream))
                using (var outputPdf = new PdfWriter(new MemoryStream()))
                {
                    var pdfDocument = new PdfDocument(expenseReport, outputPdf);

                    foreach (var attachment in attachments)
                    {
                        Stream attachmentStream;
                        if (attachment.Mimetype == "application/pdf")
                        {
                            attachmentStream = PdfTools.ToPdfStream(attachment);
                        }
                        else
                        {
                            data["attachment"] = attachment;
                            var attachmentPrepStream = RenderQwebPdfPrepareStreams("hr_expense.report_expense_sheet_img", data, resIds);
                            attachmentStream = attachmentPrepStream[expenseSheet.Id]["stream"] as Stream;
                        }

                        using (var attachmentReader = new PdfReader(attachmentStream))
                        using (var attachmentDocument = new PdfDocument(attachmentReader))
                        {
                            attachmentDocument.CopyPagesTo(1, attachmentDocument.GetNumberOfPages(), pdfDocument);
                        }

                        streamList.Add(attachmentStream);
                    }

                    pdfDocument.Close();
                    res[expenseSheet.Id]["stream"] = outputPdf.GetStream();
                }

                foreach (var str in streamList)
                {
                    str.Close();
                }
            }
        }

        return res;
    }
}
