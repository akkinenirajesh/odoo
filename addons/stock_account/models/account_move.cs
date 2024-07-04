csharp
public partial class StockAccount.AccountMove
{
    public void ComputeShowResetToDraftButton()
    {
        var move = this;
        if (Env.IsSuperUser && move.LineIds.Any(x => x.StockValuationLayerIds.Any()))
        {
            move.ShowResetToDraftButton = false;
        }
    }

    public List<StockAccount.AccountMoveLine> GetLinesOnChangeCurrency()
    {
        return this.LineIds.Where(x => x.DisplayType != "cogs").ToList();
    }

    public List<object> CopyData()
    {
        List<object> valsList = Env.CallMethod("copy_data", this, new List<object> { });
        if (!Env.Context.ContainsKey("move_reverse_cancel"))
        {
            foreach (var vals in valsList)
            {
                if (vals.ContainsKey("line_ids"))
                {
                    vals["line_ids"] = ((List<object>)vals["line_ids"]).Where(line => line is List<object> && ((List<object>)line)[0] != 0 && ((List<object>)line)[2].ContainsKey("display_type") && ((List<object>)line)[2]["display_type"] != "cogs").ToList();
                }
            }
        }

        return valsList;
    }

    public void Post()
    {
        if (Env.Context.ContainsKey("move_reverse_cancel"))
        {
            Env.CallMethod("_post", this, new List<object> { true });
            return;
        }

        Env.CallMethod("Create", Env.Model("Account.AccountMoveLine"), this._StockAccountPrepareAngloSaxonOutLinesVals());
        var posted = (StockAccount.AccountMove)Env.CallMethod("_post", this, new List<object> { true });
        if (!Env.Context.ContainsKey("skip_cogs_reconciliation"))
        {
            posted._StockAccountAngloSaxonReconcileValuation();
        }
    }

    public void ButtonDraft()
    {
        Env.CallMethod("button_draft", this);
        this.LineIds.Where(x => x.DisplayType == "cogs").ToList().ForEach(x => x.Unlink());
    }

    public void ButtonCancel()
    {
        Env.CallMethod("button_cancel", this);
        this.LineIds.Where(x => x.DisplayType == "cogs").ToList().ForEach(x => x.Unlink());
    }

    public List<object> _StockAccountPrepareAngloSaxonOutLinesVals()
    {
        List<object> linesValsList = new List<object>();
        var priceUnitPrec = Env.CallMethod("precision_get", new List<object> { "Product Price" });
        foreach (var move in this.WithCompany(this.CompanyID))
        {
            if (!move.IsSaleDocument(true) || !move.CompanyID.AngloSaxonAccounting)
            {
                continue;
            }

            foreach (var line in move.InvoiceLineIds)
            {
                if (!line._EligibleForCogs())
                {
                    continue;
                }

                var accounts = line.ProductID.ProductTmplID.GetProductAccounts(move.FiscalPositionID);
                var debitInterimAccount = accounts["stock_output"];
                var creditExpenseAccount = accounts["expense"] ?? move.JournalID.DefaultAccountID;
                if (debitInterimAccount == null || creditExpenseAccount == null)
                {
                    continue;
                }

                var sign = move.MoveType == "out_refund" ? -1 : 1;
                var priceUnit = line._StockAccountGetAngloSaxonPriceUnit();
                var amountCurrency = sign * line.Quantity * priceUnit;

                if (move.CurrencyID.IsZero(amountCurrency) || Env.CallMethod("float_is_zero", new List<object> { priceUnit, priceUnitPrec }))
                {
                    continue;
                }

                linesValsList.Add(new Dictionary<string, object>()
                {
                    {"name", line.Name.Substring(0, 64)},
                    {"move_id", move.ID},
                    {"partner_id", move.CommercialPartnerID.ID},
                    {"product_id", line.ProductID.ID},
                    {"product_uom_id", line.ProductUomID.ID},
                    {"quantity", line.Quantity},
                    {"price_unit", priceUnit},
                    {"amount_currency", -amountCurrency},
                    {"account_id", debitInterimAccount.ID},
                    {"display_type", "cogs"},
                    {"tax_ids", new List<object>()},
                    {"cogs_origin_id", line.ID},
                });

                linesValsList.Add(new Dictionary<string, object>()
                {
                    {"name", line.Name.Substring(0, 64)},
                    {"move_id", move.ID},
                    {"partner_id", move.CommercialPartnerID.ID},
                    {"product_id", line.ProductID.ID},
                    {"product_uom_id", line.ProductUomID.ID},
                    {"quantity", line.Quantity},
                    {"price_unit", -priceUnit},
                    {"amount_currency", amountCurrency},
                    {"account_id", creditExpenseAccount.ID},
                    {"analytic_distribution", line.AnalyticDistribution},
                    {"display_type", "cogs"},
                    {"tax_ids", new List<object>()},
                    {"cogs_origin_id", line.ID},
                });
            }
        }

        return linesValsList;
    }

    public List<Stock.StockMove> _StockAccountGetLastStepStockMoves()
    {
        return Env.Model("Stock.StockMove").Search<Stock.StockMove>();
    }

    public void _StockAccountAngloSaxonReconcileValuation(Product.Product product = null)
    {
        List<object> reconcilePlan = new List<object>();
        List<object> noExchangeReconcilePlan = new List<object>();
        foreach (var move in this)
        {
            if (!move.IsInvoice())
            {
                continue;
            }

            if (!move.CompanyID.AngloSaxonAccounting)
            {
                continue;
            }

            var stockMoves = move._StockAccountGetLastStepStockMoves();
            stockMoves.AddRange(stockMoves.OriginReturnedMoveID);

            if (!stockMoves.Any())
            {
                continue;
            }

            var products = product ?? move.InvoiceLineIds.Select(x => x.ProductID).ToList();
            foreach (var prod in products)
            {
                if (prod.Valuation != "real_time")
                {
                    continue;
                }

                var productAccounts = prod.ProductTmplID.GetProductAccounts();
                var productInterimAccount = move.IsSaleDocument() ? productAccounts["stock_output"] : productAccounts["stock_input"];

                if (productInterimAccount.Reconcile)
                {
                    var productAccountMoves = move.LineIds.Where(x => x.ProductID == prod && x.AccountID == productInterimAccount && !x.Reconciled).ToList();
                    var productStockMoves = stockMoves._GetAllRelatedSM(prod);
                    productAccountMoves.AddRange(productStockMoves._GetAllRelatedAML().Where(x => x.AccountID == productInterimAccount && !x.Reconciled && x.MoveID.State == "posted").ToList());

                    var correctionAmls = productAccountMoves.Where(aml => aml.MoveID.StockValuationLayerIds.Any(x => x.StockValuationLayerID != null) || (aml.DisplayType == "cogs" && aml.Quantity == 0)).ToList();
                    var invoiceAml = productAccountMoves.Where(aml => !correctionAmls.Contains(aml) && aml.MoveID == move).ToList();
                    var stockAml = productAccountMoves.Except(correctionAmls).Except(invoiceAml).ToList();
                    if (correctionAmls.Any())
                    {
                        if (correctionAmls.Sum(x => x.Balance) > 0 || correctionAmls.All(x => x.IsSameCurrency))
                        {
                            noExchangeReconcilePlan.Add(productAccountMoves);
                        }
                        else
                        {
                            noExchangeReconcilePlan.Add(invoiceAml.Concat(correctionAmls).ToList());
                            var movesToReconcile = invoiceAml.Where(x => !x.Reconciled).Concat(stockAml).ToList();
                            if (movesToReconcile.Any())
                            {
                                noExchangeReconcilePlan.Add(movesToReconcile);
                            }
                        }
                    }
                    else
                    {
                        reconcilePlan.Add(productAccountMoves);
                    }
                }
            }
        }

        Env.CallMethod("_reconcile_plan", Env.Model("Account.AccountMoveLine"), reconcilePlan);
        Env.CallMethod("_reconcile_plan", Env.Model("Account.AccountMoveLine"), noExchangeReconcilePlan, new List<object> { true });
    }

    public List<object> _GetInvoicedLotValues()
    {
        return new List<object>();
    }
}

public partial class StockAccount.AccountMoveLine
{
    public List<Stock.StockValuationLayer> StockValuationLayerIds { get; set; }

    public StockAccount.AccountMoveLine CogsOriginId { get; set; }

    public void ComputeAccountID()
    {
        if (this._EligibleForCogs() && this.MoveID.CompanyID.AngloSaxonAccounting && this.MoveID.IsPurchaseDocument())
        {
            var line = this.WithCompany(this.MoveID.JournalID.CompanyID);
            var fiscalPosition = this.MoveID.FiscalPositionID;
            var accounts = this.ProductID.ProductTmplID.GetProductAccounts(fiscalPosition);
            if (accounts["stock_input"] != null)
            {
                this.AccountID = accounts["stock_input"];
            }
        }
    }

    public bool _EligibleForCogs()
    {
        return this.ProductID.IsStorable && this.ProductID.Valuation == "real_time";
    }

    public double _GetGrossUnitPrice()
    {
        if (Env.CallMethod("float_is_zero", new List<object> { this.Quantity, this.ProductUomID.Rounding }))
        {
            return this.PriceUnit;
        }

        var priceUnit = this.PriceSubtotal / this.Quantity;
        return this.MoveID.MoveType == "in_refund" ? -priceUnit : priceUnit;
    }

    public List<Stock.StockValuationLayer> _GetStockValuationLayers(StockAccount.AccountMove move)
    {
        var valuedMoves = this._GetValuedInMoves();
        if (move.MoveType == "in_refund")
        {
            valuedMoves = valuedMoves.Where(x => x._IsOut()).ToList();
        }
        else
        {
            valuedMoves = valuedMoves.Where(x => x._IsIn()).ToList();
        }

        return valuedMoves.StockValuationLayerIds;
    }

    public List<Stock.StockMove> _GetValuedInMoves()
    {
        return Env.Model("Stock.StockMove").Search<Stock.StockMove>();
    }

    public double _StockAccountGetAngloSaxonPriceUnit()
    {
        if (this.ProductID == null)
        {
            return this.PriceUnit;
        }

        var originalLine = this.MoveID.ReversedEntryID.LineIds.FirstOrDefault(x => x.DisplayType == "cogs" && x.ProductID == this.ProductID && x.ProductUomID == this.ProductUomID && x.PriceUnit >= 0);
        return originalLine != null ? originalLine.PriceUnit : this.ProductID.WithCompany(this.CompanyID)._StockAccountGetAngloSaxonPriceUnit(this.ProductUomID);
    }

    public void InverseProductID()
    {
        if (this.DisplayType != "cogs")
        {
            Env.CallMethod("InverseProductID", this);
        }
    }
}
