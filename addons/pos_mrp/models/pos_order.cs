csharp
public partial class PosMrp.PosOrderLine {
    public virtual void _GetStockMovesToConsider(List<Stock.StockMove> stockMoves, Product.Product product) {
        var bom = Env.GetModel("Mrp.Bom")._BomFind(product, stockMoves.FirstOrDefault().Company, "phantom")[product];
        if (bom == null) {
            base._GetStockMovesToConsider(stockMoves, product);
            return;
        }
        var components = bom.Explode(product, this.Qty);
        var mlProductToConsider = (product.BomIds != null && product.BomIds.Any()) ? components.Select(comp => comp.Product.Id).ToList() : new List<int> { product.Id };
        stockMoves = stockMoves.Where(ml => mlProductToConsider.Contains(ml.ProductId) && ml.BomLineId != null).ToList();
    }
}

public partial class PosMrp.PosOrder {
    public virtual decimal _GetPosAngloSaxonPriceUnit(Product.Product product, int partnerId, decimal quantity) {
        var bom = Env.GetModel("Mrp.Bom")._BomFind(product, this.PickingIds.FirstOrDefault().MoveLineIds.FirstOrDefault().Company, "phantom")[product];
        if (bom == null) {
            return base._GetPosAngloSaxonPriceUnit(product, partnerId, quantity);
        }
        var components = bom.Explode(product, quantity);
        decimal totalPriceUnit = 0;
        foreach (var comp in components) {
            decimal priceUnit = base._GetPosAngloSaxonPriceUnit(comp.Product, partnerId, comp.Qty);
            priceUnit = comp.Product.UomId._ComputePrice(priceUnit, comp.ProductUomId);
            totalPriceUnit += priceUnit;
        }
        return totalPriceUnit;
    }
}
