csharp
public partial class StockMove
{
    public void _CalMoveWeight()
    {
        var movesWithWeight = this.Env.Search<StockMove>(m => m.ProductId.Weight > 0.00);
        foreach (var move in movesWithWeight)
        {
            move.Weight = (move.ProductQty * move.ProductId.Weight);
        }
        (this.Env.All<StockMove>() - movesWithWeight).Weight = 0;
    }

    public Dictionary<string, object> _GetNewPickingValues()
    {
        var vals = base._GetNewPickingValues();
        var carrierId = this.GroupId.SaleId.CarrierId.Id;
        vals["CarrierId"] = this.RuleId.Any(rule => rule.PropagateCarrier) && carrierId;
        return vals;
    }

    public List<object> _KeyAssignPicking()
    {
        var keys = base._KeyAssignPicking();
        return keys.Append(this.SaleLineId.OrderId.CarrierId).ToList();
    }

    public void _AutoInit()
    {
        if (!this.Env.Cr.ColumnExists("stock_move", "weight"))
        {
            this.Env.Cr.CreateColumn("stock_move", "weight", "numeric");
            this.Env.Cr.Execute("""
                UPDATE stock_move move
                SET weight = move.product_qty * product.weight
                FROM product_product product
                WHERE move.product_id = product.id
                AND move.state != 'cancel'
                """);
        }
        base._AutoInit();
    }
}

public partial class StockMoveLine
{
    public void _ComputeSalePrice()
    {
        if (this.MoveId.SaleLineId != null)
        {
            var unitPrice = this.MoveId.SaleLineId.PriceReduceTaxinc;
            var qty = this.ProductUomId.ComputeQuantity(this.Quantity, this.MoveId.SaleLineId.ProductUom);
            this.SalePrice = unitPrice * qty;
        }
        else
        {
            var unitPrice = this.ProductId.ListPrice;
            var qty = this.ProductUomId.ComputeQuantity(this.Quantity, this.ProductId.UomId);
            this.SalePrice = unitPrice * qty;
        }
        base._ComputeSalePrice();
    }

    public Dictionary<string, object> _GetAggregatedProductQuantities(Dictionary<string, object> kwargs)
    {
        var aggregatedMoveLines = base._GetAggregatedProductQuantities(kwargs);
        foreach (var aggregatedMoveLine in aggregatedMoveLines)
        {
            var hsCode = aggregatedMoveLines[aggregatedMoveLine]["product"].ProductTmplId.HsCode;
            aggregatedMoveLines[aggregatedMoveLine]["hs_code"] = hsCode;
        }
        return aggregatedMoveLines;
    }
}
