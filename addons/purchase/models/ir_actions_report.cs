csharp
public partial class Purchase_IrActionsReport {
    public virtual System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.Dictionary<string, object>> RenderQwebPdfPrepareStreams(System.Guid reportRef, System.Collections.Generic.Dictionary<string, object> data, System.Collections.Generic.List<System.Guid> resIds) {
        System.Collections.Generic.Dictionary<System.Guid, System.Collections.Generic.Dictionary<string, object>> collectedStreams = Env.Call("base.IrActionsReport", "_RenderQwebPdfPrepareStreams", reportRef, data, resIds);

        if (collectedStreams != null && resIds != null && resIds.Count == 1 && IsPurchaseOrderReport(reportRef)) {
            var purchaseOrder = Env.Call("purchase.PurchaseOrder", "Browse", resIds[0]);

            System.Collections.Generic.List<object> builders = Env.Call(purchaseOrder, "_GetEdiBuilders");

            if (builders.Count == 0) {
                return collectedStreams;
            }

            System.IO.MemoryStream pdfStream = collectedStreams[resIds[0]]["stream"] as System.IO.MemoryStream;
            byte[] pdfContent = pdfStream.ToArray();
            System.IO.MemoryStream readerBuffer = new System.IO.MemoryStream(pdfContent);
            OdooPdfFileReader reader = new OdooPdfFileReader(readerBuffer, false);
            OdooPdfFileWriter writer = new OdooPdfFileWriter();
            writer.CloneReaderDocumentRoot(reader);

            foreach (var builder in builders) {
                string xmlContent = Env.Call(builder, "_ExportOrder", purchaseOrder);
                writer.AddAttachment(Env.Call(builder, "_ExportPurchaseOrderFilename", purchaseOrder), xmlContent, "text/xml");
            }

            pdfStream.Close();
            System.IO.MemoryStream newPdfStream = new System.IO.MemoryStream();
            writer.Write(newPdfStream);
            collectedStreams[resIds[0]]["stream"] = newPdfStream;
        }

        return collectedStreams;
    }

    public virtual bool IsPurchaseOrderReport(System.Guid reportRef) {
        var report = Env.Call("ir.actions.report", "_GetReport", reportRef);
        string reportName = Env.Call(report, "ReportName");
        return reportName == "purchase.report_purchasequotation" || reportName == "purchase.report_purchaseorder";
    }
}
