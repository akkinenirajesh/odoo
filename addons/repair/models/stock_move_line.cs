csharp
public partial class StockMoveLine {
    public bool ShouldShowLotInInvoice() {
        return Env.Call("Stock.StockMoveLine", "_should_show_lot_in_invoice", this).ToBool() || this.MoveId.RepairLineType;
    }
}
