csharp
public partial class Mrp.SubcontractingPurchase.AccountMoveLine
{
    public virtual decimal GetPriceUnitValDifAndRelevantQty(decimal priceUnitValDif, decimal relevantQty)
    {
        if (Env.Get("Product.Product").Get(this.Product).CostMethod == "standard" && this.PurchaseLine != null)
        {
            decimal componentsCost = 0;
            var subcontractProduction = Env.Get("Mrp.SubcontractingPurchase.PurchaseLine").Get(this.PurchaseLine).MoveIds.GetSubcontractProduction();
            componentsCost -= subcontractProduction.MoveRawIds.StockValuationLayerIds.Sum(x => x.Value);
            decimal qty = subcontractProduction.Where(x => x.State == "done").Sum(mo => mo.ProductUomId.ComputeQuantity(mo.QtyProducing, this.ProductUomId));
            if (!Env.Tools.FloatIsZero(qty, this.ProductUomId.Rounding))
            {
                priceUnitValDif = priceUnitValDif + componentsCost / qty;
            }
        }
        return priceUnitValDif;
    }

    public virtual System.Collections.Generic.List<Mrp.SubcontractingPurchase.AccountMoveLine> GetValuedInMoves()
    {
        var res = Env.Get("Mrp.SubcontractingPurchase.AccountMoveLine").GetValuedInMoves();
        res.AddRange(res.Where(m => m.IsSubcontract).SelectMany(m => m.MoveOrigIds));
        return res;
    }
}
