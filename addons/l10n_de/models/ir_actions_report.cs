csharp
public partial class IrActionsReport
{
    public Dictionary<string, object> GetRenderingContext(IrActionsReport report, List<int> docIds, Dictionary<string, object> data)
    {
        // Call the base implementation
        var baseData = base.GetRenderingContext(report, docIds, data);

        // Add the din_header_spacing to the data
        baseData["DinHeaderSpacing"] = report.GetPaperformat().HeaderSpacing;

        return baseData;
    }
}
