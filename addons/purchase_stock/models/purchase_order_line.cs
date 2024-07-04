csharp
public partial class PurchaseStock.PurchaseOrderLine {
    public void OnDeleteStockMoves() {
        // TODO: Implement method
    }

    public void ComputeQtyReceivedMethod() {
        // TODO: Implement method
    }

    public PurchaseStock.StockMove GetPoLineMoves() {
        // TODO: Implement method
    }

    public void ComputeQtyReceived() {
        // TODO: Implement method
    }

    public void ComputeForecastedIssue() {
        // TODO: Implement method
    }

    public void Create() {
        // TODO: Implement method
    }

    public void Write(Dictionary<string, object> values) {
        // TODO: Implement method
    }

    public void ActionProductForecastReport() {
        // TODO: Implement method
    }

    public void Unlink() {
        // TODO: Implement method
    }

    public void UpdateMoveDateDeadline(DateTime newDate) {
        // TODO: Implement method
    }

    public void CreateOrUpdatePicking() {
        // TODO: Implement method
    }

    public decimal GetStockMovePriceUnit() {
        // TODO: Implement method
    }

    public decimal GetMoveDestsInitialDemand(List<PurchaseStock.StockMove> moveDests) {
        // TODO: Implement method
    }

    public List<Dictionary<string, object>> PrepareStockMoves(PurchaseStock.StockPicking picking) {
        // TODO: Implement method
    }

    public decimal GetQtyProcurement() {
        // TODO: Implement method
    }

    public void CheckOrderpointPickingType() {
        // TODO: Implement method
    }

    public Dictionary<string, object> PrepareStockMoveVals(PurchaseStock.StockPicking picking, decimal priceUnit, decimal productUomQty, PurchaseStock.ProductUom productUom) {
        // TODO: Implement method
    }

    public Dictionary<string, object> PreparePurchaseOrderLineFromProcurement(PurchaseStock.ProductProduct productId, decimal productQty, PurchaseStock.ProductUom productUom, PurchaseStock.StockLocation locationDestId, string name, string origin, PurchaseStock.ResCompany company, Dictionary<string, object> values, PurchaseStock.PurchaseOrder po) {
        // TODO: Implement method
    }

    public List<PurchaseStock.StockMove> CreateStockMoves(PurchaseStock.StockPicking picking) {
        // TODO: Implement method
    }

    public PurchaseStock.PurchaseOrderLine FindCandidate(PurchaseStock.ProductProduct productId, decimal productQty, PurchaseStock.ProductUom productUom, PurchaseStock.StockLocation locationId, string name, string origin, PurchaseStock.ResCompany company, Dictionary<string, object> values) {
        // TODO: Implement method
    }

    public Tuple<List<PurchaseStock.StockMove>, List<PurchaseStock.StockMove>> GetOutgoingIncomingMoves() {
        // TODO: Implement method
    }

    public void UpdateDatePlanned(DateTime updatedDate) {
        // TODO: Implement method
    }

    public void UpdateQtyReceivedMethod() {
        // TODO: Implement method
    }
}
