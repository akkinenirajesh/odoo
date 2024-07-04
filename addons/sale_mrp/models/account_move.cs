csharp
public partial class AccountMoveLine
{
    public float _StockAccountGetAngloSaxonPriceUnit()
    {
        float priceUnit = Env.Call<float>("Account.AccountMoveLine", "_StockAccountGetAngloSaxonPriceUnit", this);

        Sale.SaleOrderLine soLine = SaleLineIds.Count > 0 ? SaleLineIds[SaleLineIds.Count - 1] : null;
        if (soLine != null)
        {
            // We give preference to the bom in the stock moves for the sale order lines
            // If there are changes in BOMs between the stock moves creation and the
            // invoice validation a wrong price will be taken
            List<Mrp.Bom> boms = soLine.MoveIds.Where(m => m.State != "cancel").SelectMany(m => m.BomLineId.BomId).Where(b => b.Type == "phantom").ToList();
            if (boms.Count > 0)
            {
                Mrp.Bom bom = boms[0];
                bool isLineReversing = MoveId.MoveType == "out_refund";
                float qtyToInvoice = ProductUomId.ComputeQuantity(Quantity, ProductId.UomId);
                List<Account.AccountMove> accountMoves = soLine.InvoiceLines.MoveId.Where(m => m.State == "posted" && m.ReversedEntryId != null == isLineReversing).ToList();
                List<Account.AccountMoveLine> postedInvoiceLines = accountMoves.SelectMany(m => m.LineIds).Where(l => l.DisplayType == "cogs" && l.ProductId == ProductId && l.Balance > 0).ToList();
                float qtyInvoiced = postedInvoiceLines.Sum(x => x.ProductUomId.ComputeQuantity(x.Quantity, x.ProductId.UomId));
                List<Account.AccountMoveLine> reversalCogs = postedInvoiceLines.MoveId.SelectMany(m => m.ReversalMoveId.LineIds).Where(l => l.DisplayType == "cogs" && l.ProductId == ProductId && l.Balance > 0).ToList();
                qtyInvoiced -= reversalCogs.Sum(line => line.ProductUomId.ComputeQuantity(line.Quantity, line.ProductId.UomId));

                List<Stock.StockMove> moves = soLine.MoveIds;
                float averagePriceUnit = 0;
                Dictionary<int, Dictionary<string, float>> componentsQty = soLine._GetBomComponentQty(bom);
                List<Product.Product> storableComponents = Env.Search<Product.Product>("id", "in", componentsQty.Keys.ToList(), "is_storable", "=", true);
                foreach (Product.Product product in storableComponents)
                {
                    float factor = componentsQty[product.Id]["qty"];
                    List<Stock.StockMove> prodMoves = moves.Where(m => m.ProductId == product).ToList();
                    float prodQtyInvoiced = factor * qtyInvoiced;
                    float prodQtyToInvoice = factor * qtyToInvoice;
                    product = product.WithCompany(Company);
                    averagePriceUnit += factor * product._ComputeAveragePrice(prodQtyInvoiced, prodQtyToInvoice, prodMoves, isLineReversing);
                }
                priceUnit = averagePriceUnit / bom.ProductQty != 0 ? averagePriceUnit / bom.ProductQty : priceUnit;
                priceUnit = ProductId.UomId.ComputePrice(priceUnit, ProductUomId);
            }
        }
        return priceUnit;
    }
}
