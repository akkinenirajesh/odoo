csharp
public partial class SaleOrderLine {
    public void ComputeQtyToDeliver() {
        // ...
    }

    public void ComputeQtyDelivered() {
        // ...
    }

    public float ComputeUomQty(float newQty, StockMove stockMove, bool rounding) {
        // ...
    }

    public Dictionary<int, Dictionary<string, object>> GetBomComponentQty(MrpBom bom) {
        // ...
    }

    public Dictionary<string, Func<StockMove, bool>> GetIncomingOutgoingMovesFilter() {
        // ...
    }

    public float GetQtyProcurement(Dictionary<int, float> previousProductUomQty = null) {
        // ...
    }
}
