csharp
public partial class PurchaseMrp_MrpBom
{
    public virtual void CheckBomLines()
    {
        var res = Env.Call("super", "_check_bom_lines", this);
        foreach (var bom in this)
        {
            if (bom.BomLineIds.All(bl => !bl.CostShare))
            {
                continue;
            }
            if (bom.BomLineIds.Any(bl => bl.CostShare < 0))
            {
                throw new UserError("Components cost share have to be positive or equals to zero.");
            }
            if (bom.BomLineIds.Sum(bl => bl.CostShare) != 100)
            {
                throw new UserError("The total cost share for a BoM's component have to be 100");
            }
        }
    }
}

public partial class PurchaseMrp_MrpBomLine
{
    public virtual decimal GetCostShare()
    {
        if (this.CostShare)
        {
            return this.CostShare / 100;
        }
        var bom = this.BomId;
        var bomLinesWithoutCostShare = bom.BomLineIds.Where(bl => !bl.CostShare);
        return 1 / bomLinesWithoutCostShare.Count();
    }
}
