C#
public partial class StockValuationLayer
{
    public void Init()
    {
        Env.Tools.CreateIndex("stock_valuation_layer_index", "stock.valuation.layer", new string[] { "ProductId", "RemainingQty", "StockMoveId", "CompanyId", "CreateDate" });
    }

    public void ComputeWarehouseId()
    {
        if (this.StockMoveId.LocationId.Usage == "internal")
        {
            this.WarehouseId = this.StockMoveId.LocationId.WarehouseId.Id;
        }
        else
        {
            this.WarehouseId = this.StockMoveId.LocationDestId.WarehouseId.Id;
        }
    }

    public List<int> SearchWarehouseId(string operator, int value)
    {
        List<int> layerIds = Env.Ref<StockValuationLayer>().Search(new List<Tuple<string, string, object>> {
            new Tuple<string, string, object>("|", null, null),
            new Tuple<string, string, object>("StockMoveId.LocationDestId.WarehouseId", operator, value),
            new Tuple<string, string, object>("&", null, null),
            new Tuple<string, string, object>("StockMoveId.LocationId.Usage", "=", "internal"),
            new Tuple<string, string, object>("StockMoveId.LocationId.WarehouseId", operator, value),
        });
        return new List<int> { ("id", "in", layerIds) };
    }

    public void ValidateAccountingEntries()
    {
        List<Dictionary<string, object>> amVals = new List<Dictionary<string, object>>();
        Dictionary<Tuple<int, int>, HashSet<int>> amlToReconcile = new Dictionary<Tuple<int, int>, HashSet<int>>();
        foreach (StockValuationLayer svl in this)
        {
            if (!svl.WithCompany(svl.CompanyId).ProductId.Valuation == "real_time")
            {
                continue;
            }
            if (svl.CurrencyId.IsZero(svl.Value))
            {
                continue;
            }
            StockMove move = svl.StockMoveId;
            if (move == null)
            {
                move = svl.StockValuationLayerId.StockMoveId;
            }
            amVals.AddRange(move.WithCompany(svl.CompanyId).AccountEntryMove(svl.Quantity, svl.Description, svl.Id, svl.Value));
        }
        if (amVals.Count > 0)
        {
            List<AccountMove> accountMoves = Env.Ref<AccountMove>().Create(amVals);
            accountMoves.Post();
        }
        foreach (StockValuationLayer svl in this)
        {
            StockMove move = svl.StockMoveId;
            Product product = svl.ProductId;
            if (svl.CompanyId.AngloSaxonAccounting)
            {
                move.GetRelatedInvoices().StockAccountAngloSaxonReconcileValuation(product);
            }
            foreach (AccountMoveLine aml in (move | move.OriginReturnedMoveId).GetAllRelatedAml())
            {
                if (aml.Reconciled || aml.MoveId.State != "posted" || !aml.AccountId.Reconcile)
                {
                    continue;
                }
                amlToReconcile[Tuple.Create(product.Id, aml.AccountId.Id)].Add(aml.Id);
            }
        }
        foreach (HashSet<int> amlIds in amlToReconcile.Values)
        {
            Env.Ref<AccountMoveLine>().Browse(amlIds).Reconcile();
        }
    }

    public void ValidateAnalyticAccountingEntries()
    {
        foreach (StockValuationLayer svl in this)
        {
            svl.StockMoveId.AccountAnalyticEntryMove();
        }
    }

    public Dictionary<string, object> ActionOpenJournalEntry()
    {
        if (this.AccountMoveId == null)
        {
            return null;
        }
        return new Dictionary<string, object>()
        {
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "account.move" },
            { "res_id", this.AccountMoveId.Id }
        };
    }

    public Dictionary<string, object> ActionValuationAtDate()
    {
        Dictionary<string, object> context = new Dictionary<string, object>();
        if (Env.Context.ContainsKey("default_product_id"))
        {
            context["product_id"] = Env.Context["default_product_id"];
        }
        else if (Env.Context.ContainsKey("default_product_tmpl_id"))
        {
            context["product_tmpl_id"] = Env.Context["default_product_tmpl_id"];
        }
        return new Dictionary<string, object>()
        {
            { "res_model", "stock.quantity.history" },
            { "views", new List<object> { new List<object> { false, "form" } } },
            { "target", "new" },
            { "type", "ir.actions.act_window" },
            { "context", context }
        };
    }

    public Dictionary<string, object> ActionOpenReference()
    {
        if (this.StockMoveId != null)
        {
            Dictionary<string, object> action = this.StockMoveId.ActionOpenReference();
            if (action["res_model"] != "stock.move")
            {
                return action;
            }
        }
        return new Dictionary<string, object>()
        {
            { "res_model", "stock.valuation.layer" },
            { "type", "ir.actions.act_window" },
            { "views", new List<object> { new List<object> { false, "form" } } },
            { "res_id", this.Id }
        };
    }

    public Tuple<decimal, decimal> ConsumeSpecificQty(decimal qtyValued, decimal qtyToValue)
    {
        if (this == null)
        {
            return Tuple.Create(0m, 0m);
        }
        decimal rounding = this.ProductId.UomId.Rounding;
        decimal qtyToTakeOnCandidates = qtyToValue;
        decimal tmpValue = 0m;
        foreach (StockValuationLayer candidate in this)
        {
            if (Env.Tools.FloatIsZero(candidate.Quantity, rounding))
            {
                continue;
            }
            decimal candidateQuantity = Math.Abs(candidate.Quantity);
            decimal returnedQty = candidate.StockMoveId.ReturnedMoveIds.Where(sm => sm.State == "done").Sum(sm => sm.ProductUom.ComputeQuantity(sm.Quantity, this.UomId));
            candidateQuantity -= returnedQty;
            if (Env.Tools.FloatIsZero(candidateQuantity, rounding))
            {
                continue;
            }
            if (!Env.Tools.FloatIsZero(qtyValued, rounding))
            {
                decimal qtyIgnored = Math.Min(qtyValued, candidateQuantity);
                qtyValued -= qtyIgnored;
                candidateQuantity -= qtyIgnored;
                if (Env.Tools.FloatIsZero(candidateQuantity, rounding))
                {
                    continue;
                }
            }
            decimal qtyTakenOnCandidate = Math.Min(qtyToTakeOnCandidates, candidateQuantity);
            qtyToTakeOnCandidates -= qtyTakenOnCandidate;
            tmpValue += qtyTakenOnCandidate * ((candidate.Value + candidate.StockValuationLayerIds.Sum(svl => svl.Value)) / candidate.Quantity);
            if (Env.Tools.FloatIsZero(qtyToTakeOnCandidates, rounding))
            {
                break;
            }
        }
        return Tuple.Create(qtyToValue - qtyToTakeOnCandidates, tmpValue);
    }

    public Tuple<decimal, decimal> ConsumeAll(decimal qtyValued, decimal valued, decimal qtyToValue)
    {
        if (this == null)
        {
            return Tuple.Create(0m, 0m);
        }
        decimal rounding = this.ProductId.UomId.Rounding;
        decimal qtyTotal = -qtyValued;
        decimal valueTotal = -valued;
        decimal newValuedQty = 0m;
        decimal newValuation = 0m;
        foreach (StockValuationLayer svl in this)
        {
            if (Env.Tools.FloatIsZero(svl.Quantity, rounding))
            {
                continue;
            }
            decimal relevantQty = Math.Abs(svl.Quantity);
            decimal returnedQty = svl.StockMoveId.ReturnedMoveIds.Where(sm => sm.State == "done").Sum(sm => sm.ProductUom.ComputeQuantity(sm.Quantity, this.UomId));
            relevantQty -= returnedQty;
            if (Env.Tools.FloatIsZero(relevantQty, rounding))
            {
                continue;
            }
            qtyTotal += relevantQty;
            valueTotal += relevantQty * ((svl.Value + svl.StockValuationLayerIds.Sum(svl => svl.Value)) / svl.Quantity);
        }
        if (Env.Tools.FloatCompare(qtyTotal, 0, rounding) > 0)
        {
            decimal unitCost = valueTotal / qtyTotal;
            newValuedQty = Math.Min(qtyTotal, qtyToValue);
            newValuation = unitCost * newValuedQty;
        }
        return Tuple.Create(newValuedQty, newValuation);
    }
}
