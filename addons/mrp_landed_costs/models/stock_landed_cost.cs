csharp
public partial class Mrp.StockLandedCost {
    public void OnChangeTargetModel() {
        if (this.TargetModel != "manufacturing") {
            this.MrpProductionIds = null;
        }
    }

    public System.Collections.Generic.List<Mrp.StockMove> GetTargetedMoveIds() {
        var moveIds = Env.Call("stock.landed.cost", "_get_targeted_move_ids", this);
        var finishedMoveIds = this.MrpProductionIds.SelectMany(m => m.MoveFinishedIds).ToList();
        moveIds.AddRange(finishedMoveIds);
        return moveIds;
    }
}
