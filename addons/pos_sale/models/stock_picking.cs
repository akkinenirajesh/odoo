csharp
public partial class StockPicking {
    public virtual StockPicking _CreateMoveFromPosOrderLines(List<PosOrderLine> lines) {
        List<PosOrderLine> linesToUnreserve = Env.Get<PosOrderLine>().Empty();
        foreach (PosOrderLine line in lines) {
            if (line.OrderId.ShippingDate != null) {
                continue;
            }
            if (line.SaleOrderLineId.MoveIds.Any(ml => ml.LocationId.WarehouseId != line.OrderId.ConfigId.WarehouseId)) {
                continue;
            }
            linesToUnreserve.Add(line);
        }
        linesToUnreserve.SaleOrderLineId.MoveIds.Where(ml => ml.State != "cancel" && ml.State != "done")._DoUnreserve();
        return base._CreateMoveFromPosOrderLines(lines);
    }
}
