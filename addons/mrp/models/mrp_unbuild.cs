csharp
public partial class MrpUnbuild {
    public virtual void ComputeProductUomId() {
        if (this.MoId.ProductId == this.ProductId) {
            this.ProductUomId = this.MoId.ProductUomId.Id;
        } else {
            this.ProductUomId = this.ProductId.UomId.Id;
        }
    }

    public virtual void ComputeLocationId() {
        if (this.CompanyId != null) {
            var warehouse = Env.Get<Stock.Warehouse>().SearchFirst(x => x.CompanyId.Id == this.CompanyId.Id);
            if (this.LocationId.CompanyId.Id != this.CompanyId.Id) {
                this.LocationId = warehouse.LotStockId;
            }
            if (this.LocationDestId.CompanyId.Id != this.CompanyId.Id) {
                this.LocationDestId = warehouse.LotStockId;
            }
        }
    }

    public virtual void ComputeBomId() {
        if (this.MoId != null) {
            this.BomId = this.MoId.BomId;
        } else {
            this.BomId = Env.Get<Mrp.Bom>()._BomFind(this.ProductId, this.CompanyId.Id)[this.ProductId];
        }
    }

    public virtual void ComputeLotId() {
        if (this.MoId != null) {
            this.LotId = this.MoId.LotProducingId;
        }
    }

    public virtual void ComputeProductId() {
        if (this.MoId != null && this.MoId.ProductId != null) {
            this.ProductId = this.MoId.ProductId;
        }
    }

    public virtual void ComputeProductQty() {
        if (this.MoId != null) {
            if (this.HasTracking == "Serial") {
                this.ProductQty = 1;
            } else {
                this.ProductQty = this.MoId.QtyProduced;
            }
        }
    }

    public virtual MrpUnbuild Create(MrpUnbuild vals) {
        if (vals.Name == null || vals.Name == "New") {
            vals.Name = Env.Get<Ir.Sequence>().NextByCode("Mrp.MrpUnbuild") ?? "New";
        }
        return base.Create(vals);
    }

    public virtual void UnlinkExceptDone() {
        if (this.State == "Done") {
            throw new UserError("You cannot delete an unbuild order if the state is 'Done'.");
        }
    }

    protected virtual Stock.MoveLine PrepareFinishedMoveLineVals(Stock.Move finishedMove) {
        return new Stock.MoveLine {
            MoveId = finishedMove.Id,
            LotId = this.LotId.Id,
            Quantity = this.ProductQty,
            ProductId = finishedMove.ProductId.Id,
            ProductUomId = finishedMove.ProductUom.Id,
            LocationId = finishedMove.LocationId.Id,
            LocationDestId = finishedMove.LocationDestId.Id
        };
    }

    protected virtual Stock.MoveLine PrepareMoveLineVals(Stock.Move move, Stock.MoveLine originMoveLine, double takenQuantity) {
        return new Stock.MoveLine {
            MoveId = move.Id,
            LotId = originMoveLine.LotId.Id,
            Quantity = takenQuantity,
            ProductId = move.ProductId.Id,
            ProductUomId = originMoveLine.ProductUomId.Id,
            LocationId = move.LocationId.Id,
            LocationDestId = move.LocationDestId.Id
        };
    }

    public virtual MrpUnbuild ActionUnbuild() {
        this._CheckCompany();
        // remove the default_* keys that was only needed in the unbuild wizard
        Env.Context = Env.Context.CleanContext();
        if (this.ProductId.Tracking != "None" && this.LotId.Id == 0) {
            throw new UserError("You should provide a lot number for the final product.");
        }

        if (this.MoId != null && this.MoId.State != "Done") {
            throw new UserError("You cannot unbuild a undone manufacturing order.");
        }

        var consumeMoves = this._GenerateConsumeMoves();
        consumeMoves._ActionConfirm();
        var produceMoves = this._GenerateProduceMoves();
        produceMoves._ActionConfirm();
        produceMoves.Quantity = 0;

        var finishedMoves = consumeMoves.Where(m => m.ProductId == this.ProductId).ToList();
        consumeMoves = consumeMoves.Except(finishedMoves).ToList();

        if (produceMoves.Any(produceMove => produceMove.HasTracking != "None" && this.MoId == null)) {
            throw new UserError("Some of your components are tracked, you have to specify a manufacturing order in order to retrieve the correct components.");
        }

        if (consumeMoves.Any(consumeMove => consumeMove.HasTracking != "None" && this.MoId == null)) {
            throw new UserError("Some of your byproducts are tracked, you have to specify a manufacturing order in order to retrieve the correct byproducts.");
        }

        foreach (var finishedMove in finishedMoves) {
            var finishedMoveLineVals = this._PrepareFinishedMoveLineVals(finishedMove);
            Env.Get<Stock.MoveLine>().Create(finishedMoveLineVals);
        }

        // TODO: Will fail if user do more than one unbuild with lot on the same MO. Need to check what other unbuild has aready took
        var qtyAlreadyUsed = new Dictionary<Stock.MoveLine, double>();
        foreach (var move in produceMoves.Union(consumeMoves)) {
            var originalMove = move is produceMoves ? this.MoId.MoveRawIds.Where(m => m.ProductId == move.ProductId) : this.MoId.MoveFinishedIds.Where(m => m.ProductId == move.ProductId);
            if (!originalMove.Any()) {
                move.Quantity = move.ProductUom.Round(move.ProductUomQty);
                continue;
            }
            var neededQuantity = move.ProductUomQty;
            var movesLines = originalMove.SelectMany(m => m.MoveLineIds);
            if (move is produceMoves && this.LotId != null) {
                movesLines = movesLines.Where(ml => this.LotId.IsIn(ml.ProduceLineIds.Select(p => p.LotId)));
            }
            foreach (var moveLine in movesLines) {
                // Iterate over all move_lines until we unbuilded the correct quantity.
                var takenQuantity = Math.Min(neededQuantity, moveLine.Quantity - qtyAlreadyUsed.GetValueOrDefault(moveLine));
                takenQuantity = move.ProductUom.Round(takenQuantity);
                if (takenQuantity > 0) {
                    var moveLineVals = this._PrepareMoveLineVals(move, moveLine, takenQuantity);
                    Env.Get<Stock.MoveLine>().Create(moveLineVals);
                    neededQuantity -= takenQuantity;
                    qtyAlreadyUsed[moveLine] += takenQuantity;
                }
            }
        }

        (finishedMoves.Union(consumeMoves).Union(produceMoves)).Picked = true;
        finishedMoves._ActionDone();
        consumeMoves._ActionDone();
        produceMoves._ActionDone();
        var producedMoveLineIds = produceMoves.SelectMany(m => m.MoveLineIds.Where(ml => ml.Quantity > 0)).ToList();
        consumeMoves.SelectMany(m => m.MoveLineIds).ForEach(ml => ml.ProduceLineIds = producedMoveLineIds);
        if (this.MoId != null) {
            var unbuildMsg = string.Format("{0} {1} unbuilt in {2}", this.ProductQty, this.ProductUomId.Name, this._GetHtmlLink());
            this.MoId.MessagePost(body: unbuildMsg, subtypeXmlid: "mail.mt_note");
        }
        return this.Write(new MrpUnbuild { State = "Done" });
    }

    protected virtual List<Stock.Move> _GenerateConsumeMoves() {
        var moves = new List<Stock.Move>();
        if (this.MoId != null) {
            var finishedMoves = this.MoId.MoveFinishedIds.Where(move => move.State == "Done").ToList();
            var factor = this.ProductUomId.ComputeQuantity(this.ProductQty, this.MoId.ProductUomId) / this.MoId.QtyProduced;
            foreach (var finishedMove in finishedMoves) {
                moves.AddRange(this._GenerateMoveFromExistingMove(finishedMove, factor, this.LocationId, finishedMove.LocationId));
            }
        } else {
            var factor = this.ProductUomId.ComputeQuantity(this.ProductQty, this.BomId.ProductUomId) / this.BomId.ProductQty;
            moves.AddRange(this._GenerateMoveFromBomLine(this.ProductId, this.ProductUomId, this.ProductQty));
            foreach (var byproduct in this.BomId.ByproductIds) {
                if (byproduct._SkipByproductLine(this.ProductId)) {
                    continue;
                }
                var quantity = byproduct.ProductQty * factor;
                moves.AddRange(this._GenerateMoveFromBomLine(byproduct.ProductId, byproduct.ProductUomId, quantity, byproduct.Id));
            }
        }
        return moves;
    }

    protected virtual List<Stock.Move> _GenerateProduceMoves() {
        var moves = new List<Stock.Move>();
        if (this.MoId != null) {
            var rawMoves = this.MoId.MoveRawIds.Where(move => move.State == "Done").ToList();
            var factor = this.ProductUomId.ComputeQuantity(this.ProductQty, this.MoId.ProductUomId) / this.MoId.QtyProduced;
            foreach (var rawMove in rawMoves) {
                moves.AddRange(this._GenerateMoveFromExistingMove(rawMove, factor, rawMove.LocationDestId, this.LocationDestId));
            }
        } else {
            var factor = this.ProductUomId.ComputeQuantity(this.ProductQty, this.BomId.ProductUomId) / this.BomId.ProductQty;
            var boms = this.BomId.Explode(this.ProductId, factor, this.BomId.PickingTypeId);
            foreach (var line in boms.Lines) {
                moves.AddRange(this._GenerateMoveFromBomLine(line.ProductId, line.ProductUomId, line.Qty, line.Id));
            }
        }
        return moves;
    }

    protected virtual List<Stock.Move> _GenerateMoveFromExistingMove(Stock.Move move, double factor, Stock.Location locationId, Stock.Location locationDestId) {
        return new List<Stock.Move> {
            new Stock.Move {
                Name = this.Name,
                Date = this.CreateDate,
                ProductId = move.ProductId.Id,
                ProductUomQty = move.Quantity * factor,
                ProductUom = move.ProductUom.Id,
                ProcureMethod = "MakeToStock",
                LocationDestId = locationDestId.Id,
                LocationId = locationId.Id,
                WarehouseId = locationDestId.WarehouseId.Id,
                UnbuildId = this.Id,
                CompanyId = move.CompanyId.Id,
                OriginReturnedMoveId = move.Id
            }
        };
    }

    protected virtual List<Stock.Move> _GenerateMoveFromBomLine(Product.Product product, Uom.Uom productUom, double quantity, int bomLineId = 0, int byproductId = 0) {
        var productProdLocation = product.WithCompany(this.CompanyId).PropertyStockProduction;
        var locationId = bomLineId != 0 ? productProdLocation : this.LocationId;
        var locationDestId = bomLineId != 0 ? this.LocationDestId : productProdLocation;
        var warehouse = locationDestId.WarehouseId;
        return new List<Stock.Move> {
            new Stock.Move {
                Name = this.Name,
                Date = this.CreateDate,
                BomLineId = bomLineId,
                ByproductId = byproductId,
                ProductId = product.Id,
                ProductUomQty = quantity,
                ProductUom = productUom.Id,
                ProcureMethod = "MakeToStock",
                LocationDestId = locationDestId.Id,
                LocationId = locationId.Id,
                WarehouseId = warehouse.Id,
                UnbuildId = this.Id,
                CompanyId = this.CompanyId.Id
            }
        };
    }

    public virtual MrpUnbuild ActionValidate() {
        this._CheckCompany();
        var precision = Env.Get<Decimal.Precision>().PrecisionGet("Product Unit of Measure");
        var availableQty = Env.Get<Stock.Quant>()._GetAvailableQuantity(this.ProductId, this.LocationId, this.LotId, true);
        var unbuildQty = this.ProductUomId.ComputeQuantity(this.ProductQty, this.ProductId.UomId);
        if (availableQty.CompareTo(unbuildQty, precision) >= 0) {
            return this.ActionUnbuild();
        } else {
            return new Ir.Actions.ActWindow {
                Name = this.ProductId.DisplayName + ": Insufficient Quantity To Unbuild",
                ViewMode = "form",
                ResModel = "Stock.Warn.Insufficient.Qty.Unbuild",
                ViewId = Env.Ref("mrp.stock_warn_insufficient_qty_unbuild_form_view").Id,
                Type = "ir.actions.act_window",
                Context = new Dictionary<string, object> {
                    { "default_product_id", this.ProductId.Id },
                    { "default_location_id", this.LocationId.Id },
                    { "default_unbuild_id", this.Id },
                    { "default_quantity", unbuildQty },
                    { "default_product_uom_name", this.ProductId.UomName }
                },
                Target = "new"
            };
        }
    }

    protected virtual void _CheckCompany() {
        // Company check logic 
    }

    protected virtual string _GetHtmlLink() {
        // Get HTML link logic
    }
}
