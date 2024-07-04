csharp
public partial class AccountAnalyticAccount
{
    public int ProductionCount { get; set; }
    public int BomCount { get; set; }
    public int WorkorderCount { get; set; }

    public void ComputeProductionCount()
    {
        this.ProductionCount = this.ProductionIds.Count;
    }

    public void ComputeBomCount()
    {
        this.BomCount = this.BomIds.Count;
    }

    public void ComputeWorkorderCount()
    {
        var workorders = this.WorkcenterIds.SelectMany(x => x.OrderIds).Union(this.ProductionIds.SelectMany(x => x.WorkorderIds));
        this.WorkorderCount = workorders.Count();
    }

    public object ActionViewMrpProduction()
    {
        var result = new 
        {
            type = "ir.actions.act_window",
            resModel = "Mrp.Production",
            domain = new[] { new [] { "id", "in", this.ProductionIds.Select(x => x.Id).ToArray() } },
            name = "Manufacturing Orders",
            viewMode = "tree,form",
            context = new 
            {
                defaultAnalyticAccountId = this.Id
            }
        };

        if (this.ProductionIds.Count == 1)
        {
            result.viewMode = "form";
            result.resId = this.ProductionIds.First().Id;
        }

        return result;
    }

    public object ActionViewMrpBom()
    {
        var result = new 
        {
            type = "ir.actions.act_window",
            resModel = "Mrp.Bom",
            domain = new[] { new [] { "id", "in", this.BomIds.Select(x => x.Id).ToArray() } },
            name = "Bills of Materials",
            viewMode = "tree,form",
            context = new 
            {
                defaultAnalyticAccountId = this.Id
            }
        };

        if (this.BomCount == 1)
        {
            result.viewMode = "form";
            result.resId = this.BomIds.First().Id;
        }

        return result;
    }

    public object ActionViewWorkorder()
    {
        var result = new 
        {
            type = "ir.actions.act_window",
            resModel = "Mrp.Workorder",
            domain = new[] { new [] { "id", "in", (this.WorkcenterIds.SelectMany(x => x.OrderIds).Union(this.ProductionIds.SelectMany(x => x.WorkorderIds))).Select(x => x.Id).ToArray() } },
            context = new { create = false },
            name = "Work Orders",
            viewMode = "tree"
        };

        return result;
    }
}
