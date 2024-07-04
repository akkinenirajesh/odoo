csharp
public partial class AccountMoveLine {
    public AccountMoveLine() {
    }

    public virtual AccountMoveLine GetValuedInMoves() {
        this.EnsureOne();
        return Env.Model<PurchaseLine>().Get(this.PurchaseLineId).GetMoves().Where(m => m.State == "done" && m.ProductQty != 0.0).First();
    }

    public virtual double GetOutAndNotInvoicedQty(AccountMoveLine inMoves) {
        this.EnsureOne();
        if (inMoves == null) {
            return 0.0;
        }
        double amlQty = Env.Model<Uom>().Get(this.ProductUomId).ComputeQuantity(this.Quantity, Env.Model<Product>().Get(this.ProductId).UomId);
        double invoicedQty = this.PurchaseLineId.InvoiceLines.Where(l => l != this).Sum(line => Env.Model<Uom>().Get(line.ProductUomId).ComputeQuantity(line.Quantity, Env.Model<Product>().Get(line.ProductId).UomId));
        List<StockValuationLayer> layers = inMoves.GetMoves().SelectMany(m => m.StockValuationLayerIds).ToList();
        double layersQty = layers.Sum(l => l.Quantity);
        double outQty = layersQty - layers.Sum(l => l.RemainingQty);
        double totalOutAndNotInvoicedQty = Math.Max(0.0, outQty - invoicedQty);
        double outAndNotInvoicedQty = Math.Min(amlQty, totalOutAndNotInvoicedQty);
        return Env.Model<Product>().Get(this.ProductId).UomId.ComputeQuantity(outAndNotInvoicedQty, this.ProductUomId);
    }

    public virtual Tuple<List<StockValuationLayer>, List<AccountMoveLine>> ApplyPriceDifference() {
        List<StockValuationLayer> svlValsList = new List<StockValuationLayer>();
        List<AccountMoveLine> amlValsList = new List<AccountMoveLine>();
        foreach (AccountMoveLine line in this.ToList()) {
            line = line.WithCompany(line.CompanyId);
            PurchaseLine poLine = line.PurchaseLineId;
            Uom uom = line.ProductUomId != null ? Env.Model<Uom>().Get(line.ProductUomId) : Env.Model<Uom>().Get(line.ProductId.UomId);

            double quantity = poLine.QtyReceived - (poLine.QtyInvoiced - line.Quantity);
            quantity = Math.Max(Math.Min(line.Quantity, quantity), 0.0);
            if (Env.Tools.FloatUtils.FloatIsZero(quantity, precisionRounding: uom.Rounding)) {
                continue;
            }

            List<StockValuationLayer> layers = line.GetValuedInMoves().StockValuationLayerIds.Where(svl => svl.ProductId == line.ProductId && svl.StockValuationLayerId == null).ToList();
            if (layers.Count == 0) {
                continue;
            }

            Tuple<List<StockValuationLayer>, List<AccountMoveLine>> newSvlValsList = line.GeneratePriceDifferenceVals(layers);
            svlValsList.AddRange(newSvlValsList.Item1);
            amlValsList.AddRange(newSvlValsList.Item2);
        }
        return new Tuple<List<StockValuationLayer>, List<AccountMoveLine>>(Env.Model<StockValuationLayer>().Create(svlValsList), Env.Model<AccountMoveLine>().Create(amlValsList));
    }

    public virtual Tuple<List<StockValuationLayer>, List<AccountMoveLine>> GeneratePriceDifferenceVals(List<StockValuationLayer> layers) {
        this.EnsureOne();
        PurchaseLine poLine = this.PurchaseLineId;
        Uom productUom = Env.Model<Product>().Get(this.ProductId).UomId;

        List<Tuple<DateTime, AccountMoveLine, StockValuationLayer>> history = new List<Tuple<DateTime, AccountMoveLine, StockValuationLayer>>();
        foreach (StockValuationLayer layer in layers) {
            history.Add(new Tuple<DateTime, AccountMoveLine, StockValuationLayer>(layer.CreateDate, null, layer));
        }
        IrModelFields amStateField = Env.Model<IrModelFields>().GetByName("account.move", "state");
        foreach (AccountMoveLine aml in poLine.InvoiceLines) {
            AccountMove move = Env.Model<AccountMove>().Get(aml.MoveId);
            if (move.State != "posted") {
                continue;
            }
            List<MessageTrackingValue> stateTrackings = move.MessageIds.TrackingValueIds.Where(t => t.FieldId == amStateField).OrderBy(t => t.Id).ToList();
            DateTime time = stateTrackings.Count > 0 ? stateTrackings.Last().CreateDate : move.CreateDate;
            history.Add(new Tuple<DateTime, AccountMoveLine, StockValuationLayer>(time, aml, null));
        }

        history.Sort((a, b) => {
            int timeComparison = a.Item1.CompareTo(b.Item1);
            if (timeComparison != 0) {
                return timeComparison;
            } else if (a.Item2 == null && b.Item2 != null) {
                return -1;
            } else if (a.Item2 != null && b.Item2 == null) {
                return 1;
            } else {
                return (a.Item2 ?? a.Item3).Id.CompareTo((b.Item2 ?? b.Item3).Id);
            }
        });

        Dictionary<Tuple<StockValuationLayer, AccountMove>, List<double>> layersAndInvoicesQties = new Dictionary<Tuple<StockValuationLayer, AccountMove>, List<double>>();
        Dictionary<StockValuationLayer, List<double>> qtyToInvoicePerLayer = new Dictionary<StockValuationLayer, List<double>>();

        history.Add(new Tuple<DateTime, AccountMoveLine, StockValuationLayer>(DateTime.MinValue, this, null));

        foreach (Tuple<DateTime, AccountMoveLine, StockValuationLayer> item in history) {
            if (item.Item3 != null) {
                StockValuationLayer layer = item.Item3;
                double totalLayerQtyToInvoice = Math.Abs(layer.Quantity);
                List<StockValuationLayer> initialLayer = layer.StockMoveId.OriginReturnedMoveId.StockValuationLayerIds.ToList();
                if (initialLayer.Count > 0) {
                    double initialLayerRemainingQty = qtyToInvoicePerLayer[initialLayer.First()][1];
                    double commonQty = Math.Min(initialLayerRemainingQty, totalLayerQtyToInvoice);
                    qtyToInvoicePerLayer[initialLayer.First()][0] -= commonQty;
                    qtyToInvoicePerLayer[initialLayer.First()][1] -= commonQty;
                    totalLayerQtyToInvoice = Math.Max(0.0, totalLayerQtyToInvoice - commonQty);
                }
                if (Env.Tools.FloatUtils.FloatCompare(totalLayerQtyToInvoice, 0.0, precisionRounding: productUom.Rounding) > 0) {
                    qtyToInvoicePerLayer[layer] = new List<double> { totalLayerQtyToInvoice, totalLayerQtyToInvoice };
                }
            } else {
                AccountMoveLine aml = item.Item2;
                AccountMove invoice = Env.Model<AccountMove>().Get(aml.MoveId);
                AccountMove impactedInvoice = null;
                double amlQty = aml.ProductUomId.ComputeQuantity(aml.Quantity, productUom);
                if (aml.IsRefund) {
                    AccountMove reversedInvoice = invoice.ReversedEntryId;
                    if (reversedInvoice != null) {
                        double sign = -1.0;
                        impactedInvoice = reversedInvoice;
                        List<Tuple<StockValuationLayer, double>> layersToConsume = new List<Tuple<StockValuationLayer, double>>();
                        foreach (StockValuationLayer layer in layers) {
                            double remainingInvoicedQty = layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layer, reversedInvoice)][1];
                            layersToConsume.Add(new Tuple<StockValuationLayer, double>(layer, remainingInvoicedQty));
                        }
                        while (Env.Tools.FloatUtils.FloatCompare(amlQty, 0.0, precisionRounding: productUom.Rounding) > 0 && layersToConsume.Count > 0) {
                            Tuple<StockValuationLayer, double> layerAndQty = layersToConsume.First();
                            layersToConsume.RemoveAt(0);
                            if (Env.Tools.FloatUtils.FloatIsZero(layerAndQty.Item2, precisionRounding: productUom.Rounding)) {
                                continue;
                            }
                            double commonQty = Math.Min(amlQty, layerAndQty.Item2);
                            amlQty -= commonQty;
                            qtyToInvoicePerLayer[layerAndQty.Item1][1] -= sign * commonQty;
                            if (!layersAndInvoicesQties.ContainsKey(new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice))) {
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)] = new List<double> { commonQty, commonQty };
                            } else {
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][0] += commonQty;
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][1] += commonQty;
                            }
                            layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, impactedInvoice)][1] -= commonQty;
                        }
                    } else {
                        double sign = 1.0;
                        List<Tuple<StockValuationLayer, double>> layersToConsume = new List<Tuple<StockValuationLayer, double>>();
                        foreach (StockValuationLayer layer in qtyToInvoicePerLayer.Keys) {
                            if (layer.StockMoveId.IsOut()) {
                                layersToConsume.Add(new Tuple<StockValuationLayer, double>(layer, qtyToInvoicePerLayer[layer][1]));
                            }
                        }
                        while (Env.Tools.FloatUtils.FloatCompare(amlQty, 0.0, precisionRounding: productUom.Rounding) > 0 && layersToConsume.Count > 0) {
                            Tuple<StockValuationLayer, double> layerAndQty = layersToConsume.First();
                            layersToConsume.RemoveAt(0);
                            if (Env.Tools.FloatUtils.FloatIsZero(layerAndQty.Item2, precisionRounding: productUom.Rounding)) {
                                continue;
                            }
                            double commonQty = Math.Min(amlQty, layerAndQty.Item2);
                            amlQty -= commonQty;
                            qtyToInvoicePerLayer[layerAndQty.Item1][1] -= sign * commonQty;
                            if (!layersAndInvoicesQties.ContainsKey(new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice))) {
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)] = new List<double> { commonQty, commonQty };
                            } else {
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][0] += commonQty;
                                layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][1] += commonQty;
                            }
                        }
                    }
                } else {
                    double sign = 1.0;
                    List<Tuple<StockValuationLayer, double>> layersToConsume = new List<Tuple<StockValuationLayer, double>>();
                    foreach (StockValuationLayer layer in qtyToInvoicePerLayer.Keys) {
                        if (layer.StockMoveId.IsIn()) {
                            layersToConsume.Add(new Tuple<StockValuationLayer, double>(layer, qtyToInvoicePerLayer[layer][1]));
                        }
                    }
                    while (Env.Tools.FloatUtils.FloatCompare(amlQty, 0.0, precisionRounding: productUom.Rounding) > 0 && layersToConsume.Count > 0) {
                        Tuple<StockValuationLayer, double> layerAndQty = layersToConsume.First();
                        layersToConsume.RemoveAt(0);
                        if (Env.Tools.FloatUtils.FloatIsZero(layerAndQty.Item2, precisionRounding: productUom.Rounding)) {
                            continue;
                        }
                        double commonQty = Math.Min(amlQty, layerAndQty.Item2);
                        amlQty -= commonQty;
                        qtyToInvoicePerLayer[layerAndQty.Item1][1] -= sign * commonQty;
                        if (!layersAndInvoicesQties.ContainsKey(new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice))) {
                            layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)] = new List<double> { commonQty, commonQty };
                        } else {
                            layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][0] += commonQty;
                            layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layerAndQty.Item1, invoice)][1] += commonQty;
                        }
                    }
                }
            }
        }

        List<StockValuationLayer> svlValsList = new List<StockValuationLayer>();
        List<AccountMoveLine> amlValsList = new List<AccountMoveLine>();
        foreach (StockValuationLayer layer in layers) {
            double invoicingLayerQty = layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layer, Env.Model<AccountMove>().Get(this.MoveId))][1];
            if (Env.Tools.FloatUtils.FloatIsZero(invoicingLayerQty, precisionRounding: productUom.Rounding)) {
                continue;
            }
            double totalLayerQtyToInvoice = qtyToInvoicePerLayer[layer][0];
            double remainingQty = layer.RemainingQty;
            double outLayerQty = totalLayerQtyToInvoice - remainingQty;
            if (this.IsRefund) {
                double sign = -1.0;
                AccountMove reversedInvoice = Env.Model<AccountMove>().Get(this.MoveId).ReversedEntryId;
                if (reversedInvoice == null) {
                    continue;
                }
                double initialInvoicedQty = layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layer, reversedInvoice)][0];
                StockValuationLayer initialPdiffSvl = layer.StockValuationLayerIds.Where(svl => svl.AccountMoveLineId.MoveId == reversedInvoice).FirstOrDefault();
                if (initialPdiffSvl == null || Env.Tools.FloatUtils.FloatIsZero(initialInvoicedQty, precisionRounding: productUom.Rounding)) {
                    continue;
                }
                double previouslyInvoicedQty = 0.0;
                foreach (Tuple<DateTime, AccountMoveLine, StockValuationLayer> item in history) {
                    AccountMoveLine previousAml = item.Item2;
                    if (previousAml == null || previousAml.IsRefund) {
                        continue;
                    }
                    AccountMove previousInvoice = Env.Model<AccountMove>().Get(previousAml.MoveId);
                    if (previousInvoice == reversedInvoice) {
                        break;
                    }
                    previouslyInvoicedQty += layersAndInvoicesQties[new Tuple<StockValuationLayer, AccountMove>(layer, previousInvoice)][1];
                }
                double outQtyToInvoice = Math.Max(0.0, outLayerQty - previouslyInvoicedQty);
                double qtyToCorrect = Math.Max(0.0, invoicingLayerQty - outQtyToInvoice);
                if (outQtyToInvoice > 0.0) {
                    outQtyToInvoice = 0.0;
                }
                AccountMoveLine aml = initialPdiffSvl.AccountMoveLineId;
                StockValuationLayer parentLayer = initialPdiffSvl.StockValuationLayerId;
                double layerPriceUnit = parentLayer.GetLayerPriceUnit();
            } else {
                double sign = 1.0;
                double invoicedLayerQty = totalLayerQtyToInvoice - qtyToInvoicePerLayer[layer][1] - invoicingLayerQty;
                double remainingOutQtyToInvoice = Math.Max(0.0, outLayerQty - invoicedLayerQty);
                double outQtyToInvoice = Math.Min(remainingOutQtyToInvoice, invoicingLayerQty);
                double qtyToCorrect = invoicingLayerQty - outQtyToInvoice;
                double layerPriceUnit = layer.GetLayerPriceUnit();

                StockMove returnedMove = layer.StockMoveId.OriginReturnedMoveId;
                if (returnedMove != null && returnedMove.IsOut() && returnedMove.IsReturned(valuedType: "out")) {
                    layerPriceUnit = poLine.GetGrossPriceUnit();
                }

                AccountMoveLine aml = this;
                double amlGrossPriceUnit = aml.GetGrossUnitPrice();
                double amlPriceUnit = amlGrossPriceUnit / aml.CurrencyRate;
                amlPriceUnit = aml.ProductUomId.ComputePrice(amlPriceUnit, productUom);
                double unitValuationDifference = amlPriceUnit - layerPriceUnit;

                double unitValuationDifferenceCurr = unitValuationDifference * this.CurrencyRate;
                unitValuationDifferenceCurr = productUom.ComputePrice(unitValuationDifferenceCurr, this.ProductUomId);
                outQtyToInvoice = productUom.ComputeQuantity(outQtyToInvoice, this.ProductUomId);
                if (!Env.Tools.FloatUtils.FloatIsZero(unitValuationDifferenceCurr * outQtyToInvoice, precisionRounding: this.CurrencyId.Rounding)) {
                    amlValsList.AddRange(this.PreparePdiffAmlVals(outQtyToInvoice, unitValuationDifferenceCurr));
                }

                double poPuCurr = poLine.CurrencyId.Convert(poLine.PriceUnit, this.CurrencyId, this.CompanyId, this.MoveId.InvoiceDate ?? this.Date ?? Env.Tools.Date.ContextToday(this), round: false);
                double priceDifferenceCurr = poPuCurr - amlGrossPriceUnit;
                if (!Env.Tools.FloatUtils.FloatIsZero(unitValuationDifference * qtyToCorrect, precisionRounding: this.CompanyId.CurrencyId.Rounding)) {
                    StockValuationLayer svlVals = this.PreparePdiffSvlVals(layer, sign * qtyToCorrect, unitValuationDifference, priceDifferenceCurr);
                    layer.RemainingValue += svlVals.Value;
                    svlValsList.Add(svlVals);
                }
            }
        }
        return new Tuple<List<StockValuationLayer>, List<AccountMoveLine>>(svlValsList, amlValsList);
    }

    public virtual List<AccountMoveLine> PreparePdiffAmlVals(double qty, double unitValuationDifference) {
        this.EnsureOne();
        List<AccountMoveLine> valsList = new List<AccountMoveLine>();

        double sign = this.MoveId.DirectionSign;
        Account account = this.ProductId.ProductTmplId.GetProductAccounts(fiscalPos: this.MoveId.FiscalPositionId).Expense;
        if (account == null) {
            return valsList;
        }

        List<Tuple<double, Account>> priceAndAccount = new List<Tuple<double, Account>> {
            new Tuple<double, Account>(unitValuationDifference, account),
            new Tuple<double, Account>(-unitValuationDifference, Env.Model<Account>().Get(this.AccountId))
        };

        foreach (Tuple<double, Account> item in priceAndAccount) {
            valsList.Add(new AccountMoveLine {
                Name = this.Name.Substring(0, Math.Min(this.Name.Length, 64)),
                MoveId = this.MoveId.Id,
                PartnerId = this.PartnerId != null ? this.PartnerId.Id : this.MoveId.CommercialPartnerId.Id,
                CurrencyId = this.CurrencyId.Id,
                ProductId = this.ProductId.Id,
                ProductUomId = this.ProductUomId.Id,
                Balance = this.CompanyId.CurrencyId.Round((qty * item.Item1 * sign) / this.CurrencyRate),
                AccountId = item.Item2.Id,
                AnalyticDistribution = this.AnalyticDistribution,
                DisplayType = "cogs",
            });
        }
        return valsList;
    }

    public virtual StockValuationLayer PreparePdiffSvlVals(StockValuationLayer correctedLayer, double quantity, double unitCost, double pdiff) {
        this.EnsureOne();

        StockValuationLayer commonSvlVals = new StockValuationLayer {
            AccountMoveId = this.MoveId.Id,
            AccountMoveLineId = this.Id,
            CompanyId = this.CompanyId.Id,
            ProductId = this.ProductId.Id,
            Quantity = 0.0,
            UnitCost = 0.0,
            RemainingQty = 0.0,
            RemainingValue = 0.0,
            Description = this.MoveId.Name != null ? string.Format("{0} - {1}", this.MoveId.Name, this.ProductId.Name) : this.ProductId.Name,
        };
        return new StockValuationLayer {
            Quantity = quantity,
            UnitCost = unitCost,
            StockValuationLayerId = correctedLayer.Id,
            PriceDiffValue = this.CurrencyId.Round(pdiff * quantity),
            AccountMoveId = commonSvlVals.AccountMoveId,
            AccountMoveLineId = commonSvlVals.AccountMoveLineId,
            CompanyId = commonSvlVals.CompanyId,
            ProductId = commonSvlVals.ProductId,
            RemainingQty = commonSvlVals.RemainingQty,
            RemainingValue = commonSvlVals.RemainingValue,
            Description = commonSvlVals.Description,
        };
    }

    public virtual Tuple<double, double> GetPriceUnitValDifAndRelevantQty() {
        this.EnsureOne();

        List<StockMove> valuationStockMoves = this.PurchaseLineId != null ? Env.Model<StockMove>().Search(new[] {
            new Tuple<string, object>("purchase_line_id", this.PurchaseLineId.Id),
            new Tuple<string, object>("state", "done"),
            new Tuple<string, object>("product_qty", 0.0, ComparisonOperator.NotEqualTo),
        }) : new List<StockMove>();

        if (this.ProductId.CostMethod != "standard" && this.PurchaseLineId != null) {
            if (this.MoveType == "in_refund") {
                valuationStockMoves = valuationStockMoves.Where(stockMove => stockMove.IsOut()).ToList();
            } else {
                valuationStockMoves = valuationStockMoves.Where(stockMove => stockMove.IsIn()).ToList();
            }

            if (valuationStockMoves.Count == 0) {
                return new Tuple<double, double>(0.0, 0.0);
            }

            Tuple<double, double> valuationPriceUnitTotal = valuationStockMoves.GetValuationPriceAndQty(this, this.MoveId.CurrencyId);
            double valuationPriceUnit = valuationPriceUnitTotal.Item1 / valuationPriceUnitTotal.Item2;
            valuationPriceUnit = this.ProductId.UomId.ComputePrice(valuationPriceUnit, this.ProductUomId);
        } else {
            double priceUnit = this.ProductId.UomId.ComputePrice(this.ProductId.StandardPrice, this.ProductUomId);
            priceUnit = this.MoveId.MoveType == "in_refund" ? -priceUnit : priceUnit;
            DateTime valuationDate = valuationStockMoves.Count > 0 ? valuationStockMoves.Max(m => m.Date) : this.Date;
            valuationPriceUnit = this.CompanyCurrencyId.Convert(priceUnit, this.CurrencyId, this.CompanyId, valuationDate, round: false);
        }

        double priceUnit = this.GetGrossUnitPrice();
        double priceUnitValDif = priceUnit - valuationPriceUnit;

        double relevantQty = this.ProductId.CostMethod == "standard" ? this.Quantity : this.GetOutAndNotInvoicedQty(valuationStockMoves);

        return new Tuple<double, double>(priceUnitValDif, relevantQty);
    }

    private void EnsureOne() {
        if (this.ToList().Count != 1) {
            throw new InvalidOperationException("Method can only be called on a single object");
        }
    }

    private List<AccountMoveLine> ToList() {
        return new List<AccountMoveLine> { this };
    }

    private AccountMoveLine WithCompany(ResCompany company) {
        return new AccountMoveLine {
            PurchaseLineId = this.PurchaseLineId,
            Quantity = this.Quantity,
            ProductUomId = this.ProductUomId,
            ProductId = this.ProductId,
            CompanyCurrencyId = this.CompanyCurrencyId,
            CurrencyId = this.CurrencyId,
            CurrencyRate = this.CurrencyRate,
            AccountCurrencyId = this.AccountCurrencyId,
            AccountCurrencyRate = this.AccountCurrencyRate,
            AccountBalance = this.AccountBalance,
            AccountId = this.AccountId,
            PartnerId = this.PartnerId,
            MoveId = this.MoveId,
            Name = this.Name,
            AnalyticDistribution = this.AnalyticDistribution,
            Date = this.Date,
            MoveType = this.MoveType,
            CompanyId = company.Id,
        };
    }
}
