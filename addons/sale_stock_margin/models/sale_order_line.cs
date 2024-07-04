csharp
public partial class SaleOrderLine
{
    public decimal PurchasePrice { get; set; }
    public IEnumerable<SaleOrderLineMoveIds> MoveIds { get; set; }

    public void ComputePurchasePrice()
    {
        var linesWithoutMoves = Env.Model("Sale.SaleOrderLine").Browse(new List<long>());
        foreach (var line in this)
        {
            var product = Env.Model("Product.Product").Browse(line.ProductId).WithCompany(line.CompanyId);
            if (!line.HasValuedMoveIds())
            {
                linesWithoutMoves.Append(line);
            }
            else if (product != null && product.CategId.PropertyCostMethod != "standard")
            {
                var purchPrice = product.ComputeAveragePrice(0, line.ProductUomQty, line.MoveIds);
                if (line.ProductUom != product.UomId)
                {
                    purchPrice = product.UomId.ComputePrice(purchPrice, line.ProductUom);
                }
                var toCur = line.CurrencyId ?? line.OrderId.CurrencyId;
                line.PurchasePrice = line.ConvertToSolCurrency(purchPrice, product.CostCurrencyId);
            }
        }
        Env.Model("Sale.SaleOrderLine").Browse(linesWithoutMoves).ComputePurchasePrice();
    }

    private bool HasValuedMoveIds()
    {
        // TODO: implement logic for HasValuedMoveIds()
        return false;
    }

    private decimal ConvertToSolCurrency(decimal purchPrice, long costCurrencyId)
    {
        // TODO: implement logic for ConvertToSolCurrency()
        return 0;
    }
}

public class SaleOrderLineMoveIds
{
    public IEnumerable<StockValuationLayer> StockValuationLayerIds { get; set; }
    public PickingId PickingId { get; set; }
}

public class PickingId
{
    public string State { get; set; }
}
