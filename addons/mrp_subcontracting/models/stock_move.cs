csharp
public partial class MrpSubcontracting.StockMove {

    public void ComputeShowSubcontractingDetailsVisible() {
        if (!this.IsSubcontract) {
            return;
        }
        if (!this.Picked || Env.FloatIsZero(this.Quantity, this.ProductUom.Rounding)) {
            return;
        }
        var productions = GetSubcontractProduction();
        if (!productions || (productions.First().Consumption == "strict" && !productions.First().HasTrackedComponent())) {
            return;
        }
        this.ShowSubcontractingDetailsVisible = true;
    }

    public void ComputeDisplayAssignSerial() {
        if (!this.IsSubcontract) {
            return;
        }
        var productions = GetSubcontractProduction();
        if (!productions || this.HasTracking == "none") {
            return;
        }
        if (productions.HasTrackedComponent() || productions.First().Consumption != "strict") {
            this.DisplayAssignSerial = false;
        }
    }

    public void ComputeShowDetailsVisible() {
        if (!this.IsSubcontract) {
            return;
        }
        if (Env.User.IsPortal()) {
            this.ShowDetailsVisible = productions.Any(p => !p.HasBeenRecorded());
            return;
        }
        var productions = GetSubcontractProduction();
        if (!productions.HasTrackedComponent() && productions.First().Consumption == "strict") {
            return;
        }
        this.ShowDetailsVisible = true;
    }

    public void SetQuantityDone(float qty) {
        if (this.IsSubcontract && SubcontractingPossibleRecord()) {
            AutoRecordComponents(qty);
            return;
        }
        Env.StockMove.SetQuantityDone(qty);
    }

    public void SetQuantity() {
        if (this.IsSubcontract && SubcontractingPossibleRecord()) {
            var moveLineQuantities = this.MoveLineIds.Where(ml => ml.Picked).Sum(ml => ml.Quantity);
            var deltaQty = this.Quantity - moveLineQuantities;
            if (Env.FloatCompare(deltaQty, 0, this.ProductUom.Rounding) > 0) {
                AutoRecordComponents(deltaQty);
                return;
            }
            if (Env.FloatCompare(deltaQty, 0, this.ProductUom.Rounding) < 0) {
                ReduceSubcontractOrderQty(Math.Abs(deltaQty));
                return;
            }
        }
        Env.StockMove.SetQuantity();
    }

    private void AutoRecordComponents(float qty) {
        var subcontractedProductions = GetSubcontractProduction();
        var production = subcontractedProductions.Where(p => !p.HasBeenRecorded()).LastOrDefault();
        if (production == null) {
            production = subcontractedProductions.LastOrDefault();
            production = production.WithContext(allow_more: true).SplitProductions(production, new float[] { production.QtyProducing, qty }).LastOrDefault();
        }
        qty = this.ProductUom.ComputeQuantity(qty, production.ProductUomId);

        if (production.ProductTracking == "serial") {
            qty = Env.FloatRound(qty, 0, "UP");
            if (Env.FloatCompare(qty, production.ProductQty, production.ProductUomId.Rounding) < 0) {
                var remainingQty = production.ProductQty - qty;
                productions = production.WithContext(allow_more: true).SplitProductions(production, Enumerable.Repeat(1, (int)qty).Concat(new float[] { remainingQty }).ToArray()).Take(qty).ToArray();
            } else {
                productions = production.WithContext(allow_more: true).SplitProductions(production, Enumerable.Repeat(1, (int)qty).ToArray());
            }

            foreach (var production in productions) {
                production.QtyProducing = 1;
                if (!production.LotProducingId) {
                    production.ActionGenerateSerial();
                }
                production.WithContext(cancel_backorder: false).SubcontractingRecordComponent();
            }
        } else {
            production.QtyProducing = qty;
            if (Env.FloatCompare(production.QtyProducing, production.ProductQty, production.ProductUomId.Rounding) > 0) {
                Env.ChangeProductionQty.WithContext(skip_activity: true).Create(new {
                    MoId = production.Id,
                    ProductQty = qty
                }).ChangeProdQty();
            }
            if (production.ProductTracking == "lot" && !production.LotProducingId) {
                production.ActionGenerateSerial();
            }
            production.SetQtyProducing();
            production.WithContext(cancel_backorder: false).SubcontractingRecordComponent();
        }
    }

    public List<MrpSubcontracting.StockMove> CopyData(Dictionary<string, object> defaultValues = null) {
        var valsList = Env.StockMove.CopyData(defaultValues);
        for (int i = 0; i < valsList.Count; i++) {
            var move = this[i];
            var vals = valsList[i];
            if (defaultValues?.ContainsKey("LocationId") == true || !move.IsSubcontract) {
                continue;
            }
            vals["LocationId"] = move.PickingId.LocationId.Id;
        }
        return valsList;
    }

    public void Write(Dictionary<string, object> values) {
        CheckAccessIfSubcontractor(values);
        if (values.ContainsKey("ProductUomQty") && Env.Context.ContainsKey("cancel_backorder") && !Env.Context.ContainsKey("extra_move_mode")) {
            var movesToUpdate = this.Where(m => m.IsSubcontract && m.State != "draft" && m.State != "cancel" && m.State != "done" && Env.FloatCompare(m.ProductUomQty, (float)values["ProductUomQty"], m.ProductUom.Rounding) != 0);
            movesToUpdate.UpdateSubcontractOrderQty((float)values["ProductUomQty"]);
        }
        Env.StockMove.Write(values);
        if (values.ContainsKey("Date")) {
            foreach (var move in this) {
                if (move.State == "done" || move.State == "cancel" || !move.IsSubcontract) {
                    continue;
                }
                move.MoveOrigIds.ProductionId.Where(p => p.State != "done" && p.State != "cancel").Write(new {
                    DateStart = move.Date,
                    DateFinished = move.Date,
                });
            }
        }
    }

    public List<MrpSubcontracting.StockMove> Create(List<Dictionary<string, object>> valsList) {
        foreach (var vals in valsList) {
            CheckAccessIfSubcontractor(vals);
        }
        return Env.StockMove.Create(valsList);
    }

    public void ActionShowDetails() {
        if (this.State != "done" && (SubcontratingShouldBeRecord() || SubcontratingCanBeRecord())) {
            ActionRecordComponents();
            return;
        }
        var action = Env.StockMove.ActionShowDetails();
        if (this.IsSubcontract && this.GetSubcontractProduction().All(p => p.HasBeenRecorded())) {
            action["views"] = new List<object[]> { new object[] { Env.Ref("stock.view_stock_move_operations").Id, "form" } };
            action["context"] = new Dictionary<string, object> {
                { "show_lots_m2o", this.HasTracking != "none" },
                { "show_lots_text", false },
            };
        } else if (Env.User.IsPortal()) {
            action["views"] = new List<object[]> { new object[] { Env.Ref("mrp_subcontracting.mrp_subcontracting_view_stock_move_operations").Id, "form" } };
        }
        Env.Action.Execute(action);
    }

    public void ActionShowSubcontractDetails() {
        var moves = this.GetSubcontractProduction().MoveRawIds.Where(m => m.State != "cancel");
        var treeView = Env.Ref("mrp_subcontracting.mrp_subcontracting_move_tree_view");
        var formView = Env.Ref("mrp_subcontracting.mrp_subcontracting_move_form_view");
        var ctx = new Dictionary<string, object>(Env.Context) {
            { "search_default_by_product", true }
        };
        if (Env.User.IsPortal()) {
            formView = Env.Ref("mrp_subcontracting.mrp_subcontracting_portal_move_form_view");
            ctx.Add("no_breadcrumbs", false);
        }
        Env.Action.Execute(new {
            Name = $"Raw Materials for {this.ProductId.DisplayName}",
            Type = "ir.actions.act_window",
            ResModel = "stock.move",
            Views = new List<object[]> {
                new object[] { treeView.Id, "list" },
                new object[] { formView.Id, "form" },
            },
            Target = "current",
            Domain = new List<object> {
                new object[] { "id", "in", moves.Select(m => m.Id).ToArray() },
            },
            Context = ctx
        });
    }

    private void ActionRecordComponents() {
        var production = this.GetSubcontractProduction().LastOrDefault();
        var view = Env.Ref("mrp_subcontracting.mrp_production_subcontracting_form_view");
        if (Env.User.IsPortal()) {
            view = Env.Ref("mrp_subcontracting.mrp_production_subcontracting_portal_form_view");
        }
        Env.Action.Execute(new {
            Name = "Subcontract",
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "mrp.production",
            Views = new List<object[]> { new object[] { view.Id, "form" } },
            ViewId = view.Id,
            Target = "new",
            ResId = production.Id,
            Context = Env.Context,
        });
    }

    public Mrp.Bom GetSubcontractBom() {
        var bom = Env.MrpBom.WithContext(sudo: true)._BomSubcontractFind(this.ProductId, this.PickingTypeId, this.Company.Id, "subcontract", this.PickingId.PartnerId);
        return bom;
    }

    private bool SubcontratingShouldBeRecord() {
        return this.GetSubcontractProduction().Where(p => !p.HasBeenRecorded() && p.HasTrackedComponent()).Any();
    }

    private bool SubcontratingCanBeRecord() {
        return this.GetSubcontractProduction().Where(p => !p.HasBeenRecorded() && p.Consumption != "strict").Any();
    }

    private bool SubcontractingPossibleRecord() {
        return this.GetSubcontractProduction().Where(p => p.HasTrackedComponent() || p.Consumption != "strict").Any();
    }

    public List<Mrp.Production> GetSubcontractProduction() {
        return this.Where(m => m.IsSubcontract).SelectMany(m => m.MoveOrigIds.ProductionId).ToList();
    }

    private bool HasTrackedSubcontractComponents() {
        return this.GetSubcontractProduction().MoveRawIds.Any(m => m.HasTracking != "none");
    }

    public Dictionary<string, object> PrepareMoveSplitVals(float qty) {
        var vals = Env.StockMove.PrepareMoveSplitVals(qty);
        vals["LocationId"] = this.LocationId.Id;
        return vals;
    }

    public Dictionary<string, object> PrepareProcurementValues() {
        var res = Env.StockMove.PrepareProcurementValues();
        if (this.RawMaterialProductionId.SubcontractorId) {
            res["WarehouseId"] = this.PickingTypeId.WarehouseId.Id;
        }
        return res;
    }

    private bool ShouldBypassReservation(Stock.Location forcedLocation = null) {
        if (Env.StockMove.ShouldBypassReservation(forcedLocation) && this.IsSubcontract) {
            return true;
        }
        return Env.StockMove.ShouldBypassReservation(forcedLocation);
    }

    private void UpdateSubcontractOrderQty(float newQuantity) {
        foreach (var move in this) {
            var quantityToRemove = move.ProductUomQty - newQuantity;
            if (!Env.FloatIsZero(quantityToRemove, move.ProductUom.Rounding)) {
                move.ReduceSubcontractOrderQty(quantityToRemove);
            }
        }
    }

    private void ReduceSubcontractOrderQty(float quantityToRemove) {
        var productions = this.MoveOrigIds.ProductionId.Where(p => p.State != "done" && p.State != "cancel").Reverse().ToList();
        var wipProduction = this.Context.ContainsKey("transfer_qty") && productions.Count > 1 ? productions[0] : null;

        if (wipProduction != null) {
            Env.ChangeProductionQty.WithContext(skip_activity: true).Create(new {
                MoId = wipProduction.Id,
                ProductQty = wipProduction.ProductQty + quantityToRemove
            }).ChangeProdQty();
        }

        foreach (var production in productions.Except(new List<Mrp.Production> { wipProduction })) {
            if (quantityToRemove >= production.ProductQty) {
                quantityToRemove -= production.ProductQty;
                production.WithContext(skip_activity: true).ActionCancel();
            } else {
                Env.ChangeProductionQty.WithContext(skip_activity: true).Create(new {
                    MoId = production.Id,
                    ProductQty = production.ProductQty - quantityToRemove
                }).ChangeProdQty();
                break;
            }
        }
    }

    private void CheckAccessIfSubcontractor(Dictionary<string, object> vals) {
        if (Env.User.IsPortal() && !Env.Su) {
            if (vals.ContainsKey("State") && (string)vals["State"] == "done") {
                throw new Exception("Portal users cannot create a stock move with a state 'Done' or change the current state to 'Done'.");
            }
        }
    }

    private bool IsSubcontractReturn() {
        var subcontractingLocation = this.PickingId.PartnerId.WithContext(company_id: this.Company.Id).PropertyStockSubcontractor;
        return !this.IsSubcontract && this.OriginReturnedMoveId.IsSubcontract && this.LocationDestId.Id == subcontractingLocation.Id;
    }
}
