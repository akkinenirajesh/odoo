csharp
public partial class BaseIrActionsReport {

    public BaseIrActionsReport() {
        // default constructor
    }

    public void CreateAction() {
        // Implementation for CreateAction
    }

    public void UnlinkAction() {
        // Implementation for UnlinkAction
    }

    public CoreIrAttachment RetrieveAttachment(object Record) {
        // Implementation for RetrieveAttachment
    }

    public string GetWkhtmltopdfState() {
        // Implementation for GetWkhtmltopdfState
    }

    public ReportPaperFormat GetPaperFormat() {
        // Implementation for GetPaperFormat
    }

    public CoreIrUiView GetLayout() {
        // Implementation for GetLayout
    }

    public string GetReportUrl(CoreIrUiView Layout) {
        // Implementation for GetReportUrl
    }

    public List<string> BuildWkhtmltopdfArgs(
        ReportPaperFormat PaperFormatId,
        bool Landscape,
        Dictionary<string, string> SpecificPaperformatArgs,
        bool SetViewportSize) {
        // Implementation for BuildWkhtmltopdfArgs
    }

    public Tuple<List<string>, List<int>, string, string, Dictionary<string, string>> PrepareHtml(string Html, string ReportModel) {
        // Implementation for PrepareHtml
    }

    public byte[] RunWkhtmltopdf(
        List<string> Bodies,
        string ReportRef,
        string Header,
        string Footer,
        bool Landscape,
        Dictionary<string, string> SpecificPaperformatArgs,
        bool SetViewportSize) {
        // Implementation for RunWkhtmltopdf
    }

    public BaseIrActionsReport GetReportFromName(string ReportName) {
        // Implementation for GetReportFromName
    }

    public BaseIrActionsReport GetReport(string ReportRef) {
        // Implementation for GetReport
    }

    public byte[] Barcode(string BarcodeType, string Value, Dictionary<string, object> Kwargs) {
        // Implementation for Barcode
    }

    public Dictionary<string, Func<int, int, ReportlabDrawing>> GetAvailableBarcodeMasks() {
        // Implementation for GetAvailableBarcodeMasks
    }

    public byte[] RenderTemplate(string Template, Dictionary<string, object> Values) {
        // Implementation for RenderTemplate
    }

    public Tuple<byte[], string> RenderQwebPdf(string ReportRef, List<int> ResIds, Dictionary<string, object> Data) {
        // Implementation for RenderQwebPdf
    }

    public Tuple<byte[], string> RenderQwebText(string ReportRef, List<int> Docids, Dictionary<string, object> Data) {
        // Implementation for RenderQwebText
    }

    public Tuple<byte[], string> RenderQwebHtml(string ReportRef, List<int> Docids, Dictionary<string, object> Data) {
        // Implementation for RenderQwebHtml
    }

    public object GetRenderingContextModel(BaseIrActionsReport Report) {
        // Implementation for GetRenderingContextModel
    }

    public Dictionary<string, object> GetRenderingContext(BaseIrActionsReport Report, List<int> Docids, Dictionary<string, object> Data) {
        // Implementation for GetRenderingContext
    }

    public Tuple<byte[], string> Render(string ReportRef, List<int> ResIds, Dictionary<string, object> Data) {
        // Implementation for Render
    }

    public Dictionary<string, object> ReportAction(List<int> Docids, Dictionary<string, object> Data, bool Config) {
        // Implementation for ReportAction
    }

    public Dictionary<string, object> ActionConfigureExternalReportLayout(Dictionary<string, object> ReportAction, string XmlId) {
        // Implementation for ActionConfigureExternalReportLayout
    }

    public Env Env { get; set; }
}
