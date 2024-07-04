csharp
public partial class Stock_IrActionsReport
{
    public object GetRenderingContext(object report, object docids, object data)
    {
        var data1 = Env.Call("super", "_get_rendering_context", report, docids, data);
        if ((string)report.GetValue("ReportName") == "stock.report_reception_report_label" && docids == null)
        {
            docids = data["docids"];
            var docs = Env.Call(report.GetValue("Model"), "browse", docids);
            data.SetValue("DocIds", docids);
            data.SetValue("Docs", docs);
        }
        return data1;
    }
}
