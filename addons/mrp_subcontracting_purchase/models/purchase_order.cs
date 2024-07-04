csharp
public partial class MrpSubcontractingPurchase.PurchaseOrder
{
    public int SubcontractingResupplyPickingCount { get; set; }

    public void ComputeSubcontractingResupplyPickingCount()
    {
        this.SubcontractingResupplyPickingCount = GetSubcontractingResupplies().Count();
    }

    public ActionViewSubcontractingResupplyActionViewSubcontractingResupply()
    {
        return GetActionViewPicking(GetSubcontractingResupplies());
    }

    private List<Picking> GetSubcontractingResupplies()
    {
        var movesSubcontracted = this.OrderLines.SelectMany(line => line.MoveIds).Where(m => m.IsSubcontract).ToList();
        var subcontractedProductions = movesSubcontracted.SelectMany(move => move.MoveOrigIds).Select(m => m.ProductionId).ToList();
        return subcontractedProductions.SelectMany(production => production.PickingIds).ToList();
    }

    private ActionViewPickingActionViewPicking(List<Picking> pickings)
    {
        // TODO: Implement logic for ActionViewPicking
        return null;
    }

    private List<MrpProduction> GetMrpProductions(bool removeArchivedPickingTypes = true)
    {
        var productions = base.GetMrpProductions(removeArchivedPickingTypes);

        if (removeArchivedPickingTypes)
        {
            productions = productions.Where(production => production.WithContext(new Dictionary<string, object>() { { "active_test", false } }).PickingTypeId.Active).ToList();
        }

        return productions;
    }
}
