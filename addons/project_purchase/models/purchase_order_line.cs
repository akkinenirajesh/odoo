csharp
public partial class PurchaseOrderLine {
    public void ComputeAnalyticDistribution() {
        // super()._compute_analytic_distribution()
        foreach (var line in this) {
            if (line.Env.Context.ContainsKey("ProjectId")) {
                var projectId = line.Env.Context["ProjectId"];
                line.AnalyticDistribution = new Dictionary<int, int> { { Env.Project.Project.Browse(projectId).AnalyticAccountId.Id, 100 } };
            }
        }
    }

    public PurchaseOrderLine Create(List<Dictionary<string, object>> valsList) {
        // super().create(vals_list)
        var lines = Env.PurchaseOrderLine.Create(valsList);
        lines.RecomputeRecordset(new List<string>() { "AnalyticDistribution" });
        return lines;
    }
}
