csharp
public partial class MrpProduction {
    public bool ComputeShowValuation() {
        return this.Env.Model("Mrp.MrpProduction").Browse(this.Id).Get<MrpProduction>().MoveFinishedIds.Any(m => m.State == "done");
    }

    public void ComputeAnalyticDistribution() {
        if (this.BomId.AnalyticDistribution != null) {
            this.AnalyticDistribution = this.BomId.AnalyticDistribution;
            return;
        }
        this.AnalyticDistribution = this.Env.Model("Account.AccountAnalyticDistributionModel").Get<AccountAnalyticDistributionModel>().GetDistribution(new { ProductId = this.ProductId.Id, ProductCategId = this.ProductId.CategId.Id, CompanyId = this.CompanyId.Id });
    }

    public void ComputeAnalyticAccountIds() {
        if (this.AnalyticDistribution == null) return;
        this.AnalyticAccountIds = this.Env.Model("Account.AccountAnalyticAccount").Browse(this.AnalyticDistribution.SelectMany(ids => ids.Split(",")).Select(int.Parse).Distinct().ToList()).Exists();
    }

    public void Write(Dictionary<string, object> vals) {
        var res = this.Env.Model("Mrp.MrpProduction").Browse(this.Id).SuperWrite(vals);
        if (vals.ContainsKey("Name")) {
            this.MoveRawIds.AnalyticAccountLineIds.Ref = this.DisplayName;
            foreach (var workorder in this.WorkorderIds) {
                workorder.MoAnalyticAccountLineIds.Ref = this.DisplayName;
                workorder.MoAnalyticAccountLineIds.Name = $"[WC] {workorder.DisplayName}";
            }
        }
        if (vals.ContainsKey("AnalyticDistribution") && this.State != "draft") {
            this.MoveRawIds.AccountAnalyticEntryMove();
            this.WorkorderIds.CreateOrUpdateAnalyticEntry();
        }
    }

    public Dictionary<string, object> ActionViewStockValuationLayers() {
        var domain = new List<Tuple<string, object>> { new Tuple<string, object>("Id", "in", (this.MoveRawIds + this.MoveFinishedIds + this.ScrapIds.MoveIds).StockValuationLayerIds.Ids) };
        var action = this.Env.Model("Ir.Actions.Actions").Get<IrActionsActions>().GetForXmlId("stock_account.stock_valuation_layer_action");
        var context = this.Env.Context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        context["no_at_date"] = true;
        context["search_default_group_by_product_id"] = false;
        return new Dictionary<string, object> { { "domain", domain }, { "context", context } }.Merge(action);
    }

    public Dictionary<string, object> ActionViewAnalyticAccounts() {
        return new Dictionary<string, object> {
            { "type", "ir.actions.act_window" },
            { "res_model", "Account.AccountAnalyticAccount" },
            { "domain", new List<Tuple<string, object>> { new Tuple<string, object>("Id", "in", this.AnalyticAccountIds.Ids) } },
            { "name", _("Analytic Accounts") },
            { "view_mode", "tree,form" }
        };
    }

    public bool CalPrice(IEnumerable<MrpProduction> consumedMoves) {
        var res = this.Env.Model("Mrp.MrpProduction").Browse(this.Id).SuperCalPrice(consumedMoves);
        var workCenterCost = 0.0;
        var finishedMove = this.MoveFinishedIds.Where(x => x.ProductId == this.ProductId && x.State != "done" && x.State != "cancel" && x.Quantity > 0).FirstOrDefault();
        if (finishedMove != null) {
            foreach (var workOrder in this.WorkorderIds) {
                workCenterCost += workOrder.CalCost();
            }
            var quantity = finishedMove.ProductUom.ComputeQuantity(finishedMove.Quantity, finishedMove.ProductId.UomId);
            var extraCost = this.ExtraCost * quantity;
            var totalCost = -consumedMoves.SelectMany(m => m.StockValuationLayerIds).Sum(s => s.Value) + workCenterCost + extraCost;
            var byproductMoves = this.MoveByproductIds.Where(m => m.State != "done" && m.State != "cancel" && m.Quantity > 0);
            var byproductCostShare = 0.0;
            foreach (var byproduct in byproductMoves) {
                if (byproduct.CostShare == 0) continue;
                byproductCostShare += byproduct.CostShare;
                if (byproduct.ProductId.CostMethod == "fifo" || byproduct.ProductId.CostMethod == "average") {
                    byproduct.PriceUnit = totalCost * byproduct.CostShare / 100 / byproduct.ProductUom.ComputeQuantity(byproduct.Quantity, byproduct.ProductId.UomId);
                }
            }
            if (finishedMove.ProductId.CostMethod == "fifo" || finishedMove.ProductId.CostMethod == "average") {
                finishedMove.PriceUnit = totalCost * (1 - byproductCostShare / 100) / quantity;
            }
        }
        return true;
    }

    public Dictionary<string, object> GetBackorderMoVals() {
        var res = this.Env.Model("Mrp.MrpProduction").Browse(this.Id).SuperGetBackorderMoVals();
        res["ExtraCost"] = this.ExtraCost;
        return res;
    }
}
