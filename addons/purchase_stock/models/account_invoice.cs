csharp
public partial class AccountMove
{
    public virtual void _Post(bool soft = true)
    {
        if (!Env.Context.ContainsKey("move_reverse_cancel"))
        {
            Env.Model("account.move.line").Create(this._StockAccountPrepareAngloSaxonInLinesVals());
        }

        // Create correction layer and impact accounts if invoice price is different
        var stockValuationLayers = Env.Model("stock.valuation.layer").Sudo();
        var valuedLines = Env.Model("account.move.line").Sudo();
        foreach (var invoice in this)
        {
            if (invoice.Sudo().StockValuationLayerIds.Count > 0)
            {
                continue;
            }
            if (invoice.MoveType == "in_invoice" || invoice.MoveType == "in_refund" || invoice.MoveType == "in_receipt")
            {
                valuedLines |= invoice.InvoiceLineIds.Where(l => l.ProductId != null && l.ProductId.CostMethod != "standard");
            }
        }
        if (valuedLines.Count > 0)
        {
            var svls = valuedLines._ApplyPriceDifference();
            stockValuationLayers |= svls;
        }

        foreach (var (product, company) in stockValuationLayers.GroupBy(svl => (svl.ProductId, svl.CompanyId)))
        {
            product = product.WithCompany(company.Id);
            if (!product.QuantitySvl.IsZero(product.UomId.Rounding))
            {
                product.Sudo().WithContext(disable_auto_svl: true).Write(new Dictionary<string, object> { { "StandardPrice", product.ValueSvl / product.QuantitySvl } });
            }
        }

        var posted = (AccountMove)this.WithContext(skip_cogs_reconciliation: true).Call("base", "_post", soft);

        // The invoice reference is set during the super call
        foreach (var layer in stockValuationLayers)
        {
            layer.Description = $"{layer.AccountMoveLineId.MoveId.DisplayName} - {layer.ProductId.DisplayName}";
        }

        if (stockValuationLayers.Count > 0)
        {
            stockValuationLayers._ValidateAccountingEntries();
        }

        this._StockAccountAngloSaxonReconcileValuation();

        return posted;
    }

    public virtual List<Dictionary<string, object>> _StockAccountPrepareAngloSaxonInLinesVals()
    {
        var linesValsList = new List<Dictionary<string, object>>();
        var priceUnitPrec = Env.Model("decimal.precision").PrecisionGet("Product Price");

        foreach (var move in this)
        {
            if (move.MoveType != "in_invoice" && move.MoveType != "in_refund" && move.MoveType != "in_receipt" || !move.Company.AngloSaxonAccounting)
            {
                continue;
            }

            move = move.WithCompany(move.Company.Id);
            foreach (var line in move.InvoiceLineIds)
            {
                // Filter out lines being not eligible for price difference.
                // Moreover, this function is used for standard cost method only.
                if (!line.ProductId.IsStorable || line.ProductId.Valuation != "real_time" || line.ProductId.CostMethod != "standard")
                {
                    continue;
                }

                // Retrieve accounts needed to generate the price difference.
                var debitPdiffAccount = false;
                if (line.ProductId.CostMethod == "standard")
                {
                    debitPdiffAccount = line.ProductId.PropertyAccountCreditorPriceDifference ?? line.ProductId.CategId.PropertyAccountCreditorPriceDifferenceCateg;
                    debitPdiffAccount = move.FiscalPositionId.MapAccount(debitPdiffAccount);
                }
                else
                {
                    debitPdiffAccount = line.ProductId.ProductTmplId.GetProductAccounts(move.FiscalPositionId)["expense"];
                }
                if (!debitPdiffAccount)
                {
                    continue;
                }

                var (priceUnitValDif, relevantQty) = line._GetPriceUnitValDifAndRelevantQty();
                var priceSubtotal = relevantQty * priceUnitValDif;

                // We consider there is a price difference if the subtotal is not zero. In case a
                // discount has been applied, we can't round the price unit anymore, and hence we
                // can't compare them.
                if (
                    !move.CurrencyId.IsZero(priceSubtotal)
                    && float_compare(line["price_unit"], line.PriceUnit, precision_digits: priceUnitPrec) == 0
                )
                {
                    // Add price difference account line.
                    var vals = new Dictionary<string, object>
                    {
                        { "Name", line.Name.Substring(0, Math.Min(line.Name.Length, 64)) },
                        { "MoveId", move.Id },
                        { "PartnerId", line.PartnerId.Id ?? move.CommercialPartnerId.Id },
                        { "CurrencyId", line.CurrencyId.Id },
                        { "ProductId", line.ProductId.Id },
                        { "ProductUomId", line.ProductUomId.Id },
                        { "Quantity", relevantQty },
                        { "PriceUnit", priceUnitValDif },
                        { "PriceSubtotal", relevantQty * priceUnitValDif },
                        { "AmountCurrency", relevantQty * priceUnitValDif * move.DirectionSign },
                        { "Balance", line.CurrencyId._Convert(relevantQty * priceUnitValDif * move.DirectionSign, line.CompanyCurrencyId, line.CompanyId, DateTime.Today) },
                        { "AccountId", debitPdiffAccount.Id },
                        { "AnalyticDistribution", line.AnalyticDistribution },
                        { "DisplayType", "cogs" },
                    };
                    linesValsList.Add(vals);

                    // Correct the amount of the current line.
                    vals = new Dictionary<string, object>
                    {
                        { "Name", line.Name.Substring(0, Math.Min(line.Name.Length, 64)) },
                        { "MoveId", move.Id },
                        { "PartnerId", line.PartnerId.Id ?? move.CommercialPartnerId.Id },
                        { "CurrencyId", line.CurrencyId.Id },
                        { "ProductId", line.ProductId.Id },
                        { "ProductUomId", line.ProductUomId.Id },
                        { "Quantity", relevantQty },
                        { "PriceUnit", -priceUnitValDif },
                        { "PriceSubtotal", relevantQty * -priceUnitValDif },
                        { "AmountCurrency", relevantQty * -priceUnitValDif * move.DirectionSign },
                        { "Balance", line.CurrencyId._Convert(relevantQty * -priceUnitValDif * move.DirectionSign, line.CompanyCurrencyId, line.CompanyId, DateTime.Today) },
                        { "AccountId", line.AccountId.Id },
                        { "AnalyticDistribution", line.AnalyticDistribution },
                        { "DisplayType", "cogs" },
                    };
                    linesValsList.Add(vals);
                }
            }
        }
        return linesValsList;
    }

    public virtual List<Stock.StockMove> _StockAccountGetLastStepStockMoves()
    {
        var rslt = (List<Stock.StockMove>)this.Call("base", "_stock_account_get_last_step_stock_moves");
        foreach (var invoice in this.Where(x => x.MoveType == "in_invoice"))
        {
            rslt.AddRange(invoice.InvoiceLineIds.SelectMany(l => l.PurchaseLineId.MoveIds).Where(x => x.State == "done" && x.LocationId.Usage == "supplier"));
        }
        foreach (var invoice in this.Where(x => x.MoveType == "in_refund"))
        {
            rslt.AddRange(invoice.InvoiceLineIds.SelectMany(l => l.PurchaseLineId.MoveIds).Where(x => x.State == "done" && x.LocationDestId.Usage == "supplier"));
        }
        return rslt;
    }

    public virtual void _ComputeIncotermLocation()
    {
        this.Call("base", "_compute_incoterm_location");
        foreach (var move in this)
        {
            var purchaseLocations = move.LineIds.SelectMany(l => l.PurchaseLineId.OrderId.IncotermLocation);
            var incotermRes = purchaseLocations.FirstOrDefault();
            // if multiple purchase order we take an incoterm that is not false
            if (incotermRes != null)
            {
                move.IncotermLocation = incotermRes;
            }
        }
    }

    public virtual void _StockAccountAngloSaxonReconcileValuation()
    {
        // TODO: Implement this method
        // This method should reconcile the valuation layer with the account move lines
    }
}
