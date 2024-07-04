csharp
public partial class StockRule {
    public bool IsSubcontract { get; set; }

    public virtual StockRule _PushPrepareMoveCopyValues(StockMove moveToCopy, DateTime newDate) {
        var newMoveVals = Env.Call<StockRule>("_PushPrepareMoveCopyValues", moveToCopy, newDate);
        newMoveVals.IsSubcontract = false;
        return newMoveVals;
    }
}
