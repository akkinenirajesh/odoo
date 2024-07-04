C#
public partial class SaleOrder {
    public int PurchaseOrderCount { get; set; }
    public Stock.ProcurementGroup ProcurementGroupId { get; set; }

    public void ComputePurchaseOrderCount() {
        var procurementGroups = Env.GetContext().GetActiveRecord().ProcurementGroupId;
        if (procurementGroups != null) {
            var purchaseOrders = procurementGroups.StockMoveIds.SelectMany(sm => sm.CreatedPurchaseLineIds.Select(cpl => cpl.OrderId))
                .Concat(procurementGroups.StockMoveIds.SelectMany(sm => sm.MoveOrigIds.Select(moi => moi.PurchaseLineId.OrderId)))
                .Distinct();
            PurchaseOrderCount = purchaseOrders.Count();
        }
    }

    public IEnumerable<Purchase.PurchaseOrder> GetPurchaseOrders() {
        var procurementGroups = Env.GetContext().GetActiveRecord().ProcurementGroupId;
        if (procurementGroups != null) {
            return procurementGroups.StockMoveIds.SelectMany(sm => sm.CreatedPurchaseLineIds.Select(cpl => cpl.OrderId))
                .Concat(procurementGroups.StockMoveIds.SelectMany(sm => sm.MoveOrigIds.Select(moi => moi.PurchaseLineId.OrderId)))
                .Distinct();
        }
        return new List<Purchase.PurchaseOrder>();
    }
}
