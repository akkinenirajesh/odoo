csharp
public partial class MrpSubcontracting.StockPicking {
    public MrpSubcontracting.StockPicking() {
    }

    public virtual void ComputeDisplayActionRecordComponents() {
        if (this.State == "draft" || this.State == "cancel" || this.State == "done" || !this.IsSubcontract()) {
            this.DisplayActionRecordComponents = "hide";
            return;
        }

        var subcontractedMoves = this.MoveIds.Where(m => m.IsSubcontract).ToList();
        if (subcontractedMoves.SubcontratingShouldBeRecord()) {
            this.DisplayActionRecordComponents = "mandatory";
            return;
        }

        if (subcontractedMoves.SubcontratingCanBeRecord()) {
            this.DisplayActionRecordComponents = "facultative";
        }
    }

    public virtual void ComputeLocationId() {
        // Super Call
        // ...

        if (this.PickingTypeId == this.PickingTypeId.WarehouseId.SubcontractingResupplyTypeId && this.PartnerId.PropertyStockSubcontractor != null) {
            this.LocationDestId = this.PartnerId.PropertyStockSubcontractor;
        }
    }

    public virtual void ActionDone() {
        // Super Call
        // ...

        foreach (var move in this.MoveIds) {
            if (!move.IsSubcontract) {
                continue;
            }

            var productions = move.GetSubcontractProduction();
            var recordedProductions = productions.Where(p => p.HasbeenRecorded()).ToList();
            var recordedQty = recordedProductions.Sum(p => p.QtyProducing);
            var smDoneQty = productions.GetSubcontractMove().Where(m => m.Picked).Sum(m => m.Quantity);

            var rounding = Env.GetDecimalPrecision("Product Unit of Measure");

            if (rounding.CompareTo(move.ProductUomQty, move.Quantity) > 0 && Env.Context.ContainsKey("cancel_backorder")) {
                move.UpdateSubcontractOrderQty(move.Quantity);
            }

            if (rounding.CompareTo(recordedQty, smDoneQty) >= 0) {
                continue;
            }

            var production = productions.Except(recordedProductions).ToList();

            if (!production.Any()) {
                continue;
            }

            if (production.Count > 1) {
                throw new UserError("There shouldn't be multiple productions to record for the same subcontracted move.");
            }

            var quantityDoneMove = move.ProductUom.ComputeQuantity(move.Quantity, production[0].ProductUomId);

            if (rounding.CompareTo(production[0].ProductQty, quantityDoneMove) == -1) {
                var changeQty = Env.Create<ChangeProductionQty>(new ChangeProductionQty() {
                    MoId = production[0].Id,
                    ProductQty = quantityDoneMove
                });

                changeQty.ChangeProdQty(skipActivity: true);
            }

            var amounts = move.MoveLineIds.Select(ml => ml.Quantity).ToList();
            var lenAmounts = amounts.Count;

            productions = production[0].SplitProductions(new Dictionary<MrpProduction, List<decimal>>() {
                { production[0], amounts }
            }, setConsumedQty: true);

            productions.MoveFinishedIds.MoveLineIds.ForEach(ml => ml.Quantity = 0);

            var i = 0;
            foreach (var prod in productions) {
                if (move.MoveLineIds[i].LotId != null) {
                    prod.LotProducingId = move.MoveLineIds[i].LotId;
                }

                prod.QtyProducing = prod.ProductQty;
                prod.SetQtyProducing();
                i++;
            }

            productions.Take(lenAmounts).ForEach(prod => prod.SubcontractingHasBeenRecorded = true);
        }

        foreach (var picking in this) {
            var productionsToDone = picking.GetSubcontractProduction().SubcontractingFilterToDone();
            productionsToDone.SubcontractSanityCheck();

            if (!productionsToDone.Any()) {
                continue;
            }

            productionsToDone = productionsToDone.sudo();
            var productionIdsBackorder = new List<long>();

            if (!Env.Context.ContainsKey("cancel_backorder")) {
                productionIdsBackorder = productionsToDone.Where(mo => mo.State == "progress").Select(mo => mo.Id).ToList();
            }

            productionsToDone.ButtonMarkDone(moIdsToBackorder: productionIdsBackorder);

            var minimumDate = picking.MoveLineIds.Min(ml => ml.Date);
            var productionMoves = productionsToDone.MoveRawIds.Union(productionsToDone.MoveFinishedIds).ToList();
            productionMoves.ForEach(pm => pm.Date = minimumDate.AddSeconds(-1));
            productionMoves.ForEach(pm => pm.MoveLineIds.ForEach(ml => ml.Date = minimumDate.AddSeconds(-1)));
        }
    }

    public virtual void ActionRecordComponents() {
        if (this.MoveIds.Any(m => m.IsSubcontract && m.SubcontratingShouldBeRecord())) {
            var move = this.MoveIds.FirstOrDefault(m => m.IsSubcontract && m.SubcontratingShouldBeRecord());
            move.ActionRecordComponents();
            return;
        }

        if (this.MoveIds.Any(m => m.IsSubcontract && m.SubcontratingCanBeRecord())) {
            var move = this.MoveIds.FirstOrDefault(m => m.IsSubcontract && m.SubcontratingCanBeRecord());
            move.ActionRecordComponents();
            return;
        }

        throw new UserError("Nothing to record");
    }

    public virtual bool IsSubcontract() {
        return this.PickingTypeId.Code == "incoming" && this.MoveIds.Any(m => m.IsSubcontract);
    }

    public virtual List<MrpProduction> GetSubcontractProduction() {
        return this.MoveIds.GetSubcontractProduction();
    }

    public virtual StockWarehouse GetWarehouse(MrpSubcontracting.StockMove subcontractMove) {
        return subcontractMove.WarehouseId ?? this.PickingTypeId.WarehouseId ?? subcontractMove.MoveDestIds.PickingTypeId.WarehouseId;
    }

    public virtual MrpProduction PrepareSubcontractMoVals(MrpSubcontracting.StockMove subcontractMove, MrpBom bom) {
        var group = Env.Create<ProcurementGroup>(new ProcurementGroup() {
            Name = this.Name,
            PartnerId = this.PartnerId.Id
        });

        var product = subcontractMove.ProductId;
        var warehouse = this.GetWarehouse(subcontractMove);

        var moSubcontract = new MrpProduction() {
            CompanyId = subcontractMove.CompanyId.Id,
            ProcurementGroupId = group.Id,
            SubcontractorId = subcontractMove.PickingId.PartnerId.CommercialPartnerId.Id,
            PickingIds = new List<long>() { subcontractMove.PickingId.Id },
            ProductId = product.Id,
            ProductUomId = subcontractMove.ProductUom.Id,
            BomId = bom.Id,
            LocationSrcId = subcontractMove.PickingId.PartnerId.WithCompany(subcontractMove.CompanyId).PropertyStockSubcontractor.Id,
            LocationDestId = subcontractMove.PickingId.PartnerId.WithCompany(subcontractMove.CompanyId).PropertyStockSubcontractor.Id,
            ProductQty = subcontractMove.ProductUomQty ?? subcontractMove.Quantity,
            PickingTypeId = warehouse.SubcontractingTypeId.Id,
            DateStart = subcontractMove.Date.AddDays(-bom.ProduceDelay)
        };

        return moSubcontract;
    }

    public virtual void SubcontractedProduce(List<Tuple<MrpSubcontracting.StockMove, MrpBom>> subcontractDetails) {
        var groupMove = new Dictionary<long, MrpSubcontracting.StockMove>();
        var groupByCompany = new Dictionary<long, List<MrpProduction>>();

        foreach (var detail in subcontractDetails) {
            var move = detail.Item1;
            var bom = detail.Item2;

            if (move.MoveOrigIds.ProductionId != null) {
                continue;
            }

            var quantity = move.ProductQty ?? move.Quantity;

            if (rounding.CompareTo(quantity, 0) <= 0) {
                continue;
            }

            var moSubcontract = this.PrepareSubcontractMoVals(move, bom);
            groupMove.Add(moSubcontract.ProcurementGroupId, move);

            if (!groupByCompany.ContainsKey(move.CompanyId.Id)) {
                groupByCompany.Add(move.CompanyId.Id, new List<MrpProduction>());
            }

            groupByCompany[move.CompanyId.Id].Add(moSubcontract);
        }

        var allMo = new List<MrpProduction>();

        foreach (var kvp in groupByCompany) {
            var groupedMo = Env.Create<MrpProduction>(kvp.Value, kvp.Key);
            allMo.AddRange(groupedMo);
        }

        allMo = allMo.OrderBy(mo => mo.Id).ToList();
        allMo.ForEach(mo => mo.ActionConfirm());

        foreach (var mo in allMo) {
            var move = groupMove[mo.ProcurementGroupId];
            mo.DateFinished = move.Date;

            var finishedMove = mo.MoveFinishedIds.Where(m => m.ProductId == move.ProductId).ToList();
            finishedMove.ForEach(fm => fm.MoveDestIds.Add(new StockMove(move.Id)));
        }

        allMo.ForEach(mo => mo.ActionAssign());
    }
}
