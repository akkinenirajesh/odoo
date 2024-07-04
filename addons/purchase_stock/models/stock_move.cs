C#
public partial class PurchaseStock.StockMove
{
    // All methods are written here.
    public void ComputePartnerId()
    {
        // dropshipped moves should have their partner_ids directly set
        var notDropshippedMoves = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove").Where(m => !m.IsDropshipped()).ToList();
        this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._ComputePartnerId(notDropshippedMoves);
    }

    public bool ShouldIgnorePolPrice()
    {
        return this.OriginReturnedMoveId != null || this.PurchaseLineId == null || this.ProductId == null;
    }

    public decimal GetPriceUnit()
    {
        if (this.ShouldIgnorePolPrice())
        {
            return this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetPriceUnit(this);
        }
        decimal priceUnitPrec = this.Env.Ref<DecimalPrecision>("DecimalPrecision").GetPrecision("Product Price");
        var line = this.PurchaseLineId;
        var order = line.OrderId;
        decimal receivedQty = line.QtyReceived;
        if (this.State == "done")
        {
            receivedQty -= this.ProductUom.ComputeQuantity(this.Quantity, line.ProductUom, "HALF-UP");
        }
        if (line.ProductId.PurchaseMethod == "purchase" && this.Env.Ref<DecimalPrecision>("DecimalPrecision").Compare(line.QtyInvoiced, receivedQty, line.ProductUom.Rounding) > 0)
        {
            var moveLayer = line.MoveIds.StockValuationLayerIds;
            var invoicedLayer = line.InvoiceLines.StockValuationLayerIds;
            // value on valuation layer is in company's currency, while value on invoice line is in order's currency
            decimal receiptValue = 0;
            if (moveLayer != null)
            {
                receiptValue += moveLayer.Sum(l => l.CurrencyId.Convert(l.Value, order.CurrencyId, order.CompanyId, l.CreateDate, false));
            }
            if (invoicedLayer != null)
            {
                receiptValue += invoicedLayer.Sum(l => l.CurrencyId.Convert(l.Value, order.CurrencyId, order.CompanyId, l.CreateDate, false));
            }
            decimal totalInvoicedValue = 0;
            decimal invoicedQty = 0;
            foreach (var invoiceLine in line.InvoiceLines)
            {
                if (invoiceLine.MoveId.State != "posted")
                {
                    continue;
                }
                // Adjust unit price to account for discounts before adding taxes.
                decimal adjustedUnitPrice = invoiceLine.PriceUnit * (1 - (invoiceLine.Discount / 100)) == 0 ? invoiceLine.PriceUnit : invoiceLine.PriceUnit;
                decimal invoiceLineValue = 0;
                if (invoiceLine.TaxIds != null)
                {
                    invoiceLineValue = invoiceLine.TaxIds.WithContext(false).ComputeAll(adjustedUnitPrice, invoiceLine.CurrencyId, invoiceLine.Quantity)["total_void"];
                }
                else
                {
                    invoiceLineValue = adjustedUnitPrice * invoiceLine.Quantity;
                }
                totalInvoicedValue += invoiceLine.CurrencyId.Convert(invoiceLineValue, order.CurrencyId, order.CompanyId, invoiceLine.MoveId.InvoiceDate, false);
                invoicedQty += invoiceLine.ProductUomId.ComputeQuantity(invoiceLine.Quantity, line.ProductId.UomId);
            }
            // TODO currency check
            decimal remainingValue = totalInvoicedValue - receiptValue;
            // TODO qty_received in product uom
            decimal remainingQty = invoicedQty - line.ProductUom.ComputeQuantity(receivedQty, line.ProductId.UomId);
            if (order.CurrencyId != order.CompanyId.CurrencyId && remainingValue != 0 && remainingQty != 0)
            {
                // will be rounded during currency conversion
                decimal priceUnit = remainingValue / remainingQty;
                return priceUnit;
            }
            else if (remainingValue != 0 && remainingQty != 0)
            {
                decimal priceUnit = this.Env.Ref<DecimalPrecision>("DecimalPrecision").Round(remainingValue / remainingQty, priceUnitPrec);
                return priceUnit;
            }
            else
            {
                decimal priceUnit = line.GetGrossPriceUnit();
                return priceUnit;
            }
        }
        else
        {
            decimal priceUnit = line.GetGrossPriceUnit();
            return priceUnit;
        }
        if (order.CurrencyId != order.CompanyId.CurrencyId)
        {
            // The date must be today, and not the date of the move since the move move is still
            // in assigned state. However, the move date is the scheduled date until move is
            // done, then date of actual move processing. See:
            // https://github.com/odoo/odoo/blob/2f789b6863407e63f90b3a2d4cc3be09815f7002/addons/stock/models/stock_move.py#L36
            decimal priceUnit = order.CurrencyId.Convert(priceUnit, order.CompanyId.CurrencyId, order.CompanyId, this.Env.Ref<Date>("Date").ContextToday(this), false);
            return priceUnit;
        }
        return priceUnit;
    }

    public List<object> GenerateValuationLinesData(int partnerId, decimal qty, decimal debitValue, decimal creditValue, int debitAccountId, int creditAccountId, int svlId, string description)
    {
        // Overridden from stock_account to support amount_currency on valuation lines generated from po
        List<object> rslt = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GenerateValuationLinesData(this, partnerId, qty, debitValue, creditValue, debitAccountId, creditAccountId, svlId, description);
        var purchaseCurrency = this.PurchaseLineId.CurrencyId;
        var companyCurrency = this.CompanyId.CurrencyId;
        if (this.PurchaseLineId == null || purchaseCurrency == companyCurrency)
        {
            return rslt;
        }
        var svl = this.Env.Ref<StockValuationLayer>("StockValuationLayer").Browse(svlId);
        if (svl.AccountMoveLineId == null)
        {
            rslt["credit_line_vals"]["amount_currency"] = companyCurrency.Convert(
                rslt["credit_line_vals"]["balance"],
                purchaseCurrency,
                this.CompanyId,
                this.Date
            );
            rslt["debit_line_vals"]["amount_currency"] = companyCurrency.Convert(
                rslt["debit_line_vals"]["balance"],
                purchaseCurrency,
                this.CompanyId,
                this.Date
            );
            rslt["debit_line_vals"]["currency_id"] = purchaseCurrency.Id;
            rslt["credit_line_vals"]["currency_id"] = purchaseCurrency.Id;
        }
        else
        {
            rslt["credit_line_vals"]["amount_currency"] = 0;
            rslt["debit_line_vals"]["amount_currency"] = 0;
            rslt["debit_line_vals"]["currency_id"] = purchaseCurrency.Id;
            rslt["credit_line_vals"]["currency_id"] = purchaseCurrency.Id;
            if (svl.PriceDiffValue == 0)
            {
                return rslt;
            }
            // The idea is to force using the company currency during the reconciliation process
            rslt["debit_line_vals_curr"] = new Dictionary<string, object>()
            {
                { "name", _("Currency exchange rate difference") },
                { "product_id", this.ProductId.Id },
                { "quantity", 0 },
                { "product_uom_id", this.ProductId.UomId.Id },
                { "partner_id", partnerId },
                { "balance", 0 },
                { "account_id", debitAccountId },
                { "currency_id", purchaseCurrency.Id },
                { "amount_currency", -svl.PriceDiffValue }
            };
            rslt["credit_line_vals_curr"] = new Dictionary<string, object>()
            {
                { "name", _("Currency exchange rate difference") },
                { "product_id", this.ProductId.Id },
                { "quantity", 0 },
                { "product_uom_id", this.ProductId.UomId.Id },
                { "partner_id", partnerId },
                { "balance", 0 },
                { "account_id", creditAccountId },
                { "currency_id", purchaseCurrency.Id },
                { "amount_currency", svl.PriceDiffValue }
            };
        }
        return rslt;
    }

    public List<object> AccountEntryMove(decimal qty, string description, int svlId, decimal cost)
    {
        // In case of a PO return, if the value of the returned product is
        // different from the purchased one, we need to empty the stock_in account
        // with the difference
        List<object> amValsList = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._AccountEntryMove(this, qty, description, svlId, cost);
        var returnedMove = this.OriginReturnedMoveId;
        bool pdiffExists = (this | returnedMove).StockValuationLayerIds.StockValuationLayerIds.AccountMoveLineId != null;

        if (amValsList == null || this.PurchaseLineId == null || pdiffExists || this.Env.Ref<DecimalPrecision>("DecimalPrecision").IsZero(qty, this.ProductId.UomId.Rounding))
        {
            return amValsList;
        }

        var layer = this.Env.Ref<StockValuationLayer>("StockValuationLayer").Browse(svlId);
        returnedMove = this.OriginReturnedMoveId;

        if (returnedMove != null && this.IsOut() && this.IsReturned("out"))
        {
            var returnedLayer = returnedMove.StockValuationLayerIds.Where(svl => svl.StockValuationLayerId == null).FirstOrDefault();
            decimal unitDiff = layer.GetLayerPriceUnit() - returnedLayer.GetLayerPriceUnit();
        }
        else if (returnedMove != null && returnedMove.IsOut() && returnedMove.IsReturned("out"))
        {
            var returnedLayer = returnedMove.StockValuationLayerIds.Where(svl => svl.StockValuationLayerId == null).FirstOrDefault();
            decimal unitDiff = returnedLayer.GetLayerPriceUnit() - this.PurchaseLineId.GetGrossPriceUnit();
        }
        else
        {
            return amValsList;
        }

        decimal diff = unitDiff * qty;
        var company = this.PurchaseLineId.CompanyId;
        if (company.CurrencyId.IsZero(diff))
        {
            return amValsList;
        }

        var sm = this.WithContext(company).WithContext("is_returned", true);
        var accounts = sm.ProductId.ProductTmplId.GetProductAccounts();
        int accExpId = accounts["expense"].Id;
        int accStockInId = accounts["stock_input"].Id;
        int journalId = accounts["stock_journal"].Id;
        var vals = sm._PrepareAccountMoveVals(accExpId, accStockInId, journalId, qty, description, false, diff);
        amValsList.Add(vals);

        return amValsList;
    }

    public Dictionary<string, object> PrepareMoveSplitVals(decimal uomQty)
    {
        Dictionary<string, object> vals = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._PrepareMoveSplitVals(this, uomQty);
        vals["PurchaseLineId"] = this.PurchaseLineId.Id;
        return vals;
    }

    public void CleanMerged()
    {
        this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._CleanMerged(this);
        this.Write(new Dictionary<string, object>()
        {
            { "CreatedPurchaseLineIds", new List<object>() { new Dictionary<string, object>() { { "command", "clear" } } } }
        });
    }

    public List<object> GetUpstreamDocumentsAndResponsibles(List<object> visited)
    {
        var createdPl = this.CreatedPurchaseLineIds.Where(cpl => cpl.State != "done" && cpl.State != "cancel" && (cpl.State != "draft" || this.Context.ContainsKey("include_draft_documents"))).ToList();
        if (createdPl != null)
        {
            return createdPl.Select(pl => new object[] { pl.OrderId, pl.OrderId.UserId, visited }).ToList();
        }
        else if (this.PurchaseLineId != null && this.PurchaseLineId.State != "done" && this.PurchaseLineId.State != "cancel")
        {
            return new List<object>() { new object[] { this.PurchaseLineId.OrderId, this.PurchaseLineId.OrderId.UserId, visited } };
        }
        else
        {
            return this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetUpstreamDocumentsAndResponsibles(this, visited);
        }
    }

    public List<object> GetRelatedInvoices()
    {
        // Overridden to return the vendor bills related to this stock move.
        List<object> rslt = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetRelatedInvoices(this);
        rslt.AddRange(this.PickingId.PurchaseId.InvoiceIds.Where(x => x.State == "posted").ToList());
        return rslt;
    }

    public object GetSourceDocument()
    {
        object res = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetSourceDocument(this);
        return this.PurchaseLineId.OrderId != null ? this.PurchaseLineId.OrderId : res;
    }

    public Tuple<decimal, decimal> GetValuationPriceAndQty(AccountMoveLine relatedAml, Currency toCurr)
    {
        decimal valuationPriceUnitTotal = 0;
        decimal valuationTotalQty = 0;
        foreach (var valStockMove in this)
        {
            // In case val_stock_move is a return move, its valuation entries have been made with the
            // currency rate corresponding to the original stock move
            DateTime valuationDate = valStockMove.OriginReturnedMoveId.Date ?? valStockMove.Date;
            var svl = valStockMove.WithContext(false).StockValuationLayerIds.Where(l => l.Quantity != 0).ToList();
            decimal layersQty = svl.Sum(l => l.Quantity);
            decimal layersValues = svl.Sum(l => l.Value);
            valuationPriceUnitTotal += relatedAml.CompanyCurrencyId.Convert(layersValues, toCurr, relatedAml.CompanyId, valuationDate, false);
            valuationTotalQty += layersQty;
        }
        if (this.Env.Ref<DecimalPrecision>("DecimalPrecision").IsZero(valuationTotalQty, relatedAml.ProductUomId.Rounding ?? relatedAml.ProductId.UomId.Rounding))
        {
            throw new UserError(_("Odoo is not able to generate the anglo saxon entries. The total valuation of %s is zero.", relatedAml.ProductId.DisplayName));
        }
        return new Tuple<decimal, decimal>(valuationPriceUnitTotal, valuationTotalQty);
    }

    public bool IsPurchaseReturn()
    {
        return this.LocationDestId.Usage == "supplier";
    }

    public List<object> GetAllRelatedAml()
    {
        // The back and for between account_move and account_move_line is necessary to catch the
        // additional lines from a cogs correction
        List<object> rslt = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetAllRelatedAml(this);
        rslt.AddRange(this.PurchaseLineId.InvoiceLines.MoveId.LineIds.Where(aml => aml.ProductId == this.PurchaseLineId.ProductId).ToList());
        return rslt;
    }

    public List<object> GetAllRelatedSm(Product product)
    {
        List<object> rslt = this.Env.Ref<PurchaseStock.StockMove>("PurchaseStock.StockMove")._GetAllRelatedSm(this, product);
        rslt.AddRange(this.Where(m => m.PurchaseLineId.ProductId == product).ToList());
        return rslt;
    }

    public Tuple<int, int> GetPurchaseLineAndPartnerFromChain()
    {
        var movesToCheck = new Queue<PurchaseStock.StockMove>(this);
        var seenMoves = new HashSet<PurchaseStock.StockMove>();
        while (movesToCheck.Count > 0)
        {
            var currentMove = movesToCheck.Dequeue();
            if (currentMove.PurchaseLineId != null)
            {
                return new Tuple<int, int>(currentMove.PurchaseLineId.Id, currentMove.PickingId.PartnerId.Id);
            }
            seenMoves.Add(currentMove);
            movesToCheck.EnqueueRange(currentMove.MoveOrigIds.Where(move => !movesToCheck.Contains(move) && !seenMoves.Contains(move)).ToList());
        }
        return null;
    }
}
