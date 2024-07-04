csharp
public partial class StockPackageLevel {
    public void ComputeIsDone() {
        if (this.IsFreshPackage) {
            this.IsDone = true;
        } else {
            this.IsDone = this.CheckMoveLinesMapQuantPackage(this.PackageId, true);
        }
    }

    public void SetIsDone() {
        if (this.IsDone) {
            if (!this.IsFreshPackage) {
                var mlUpdateDict = new Dictionary<StockMoveLine, decimal>();
                Env.GetModel("Stock.StockPicking").GetById(this.PickingId).GetCollection<StockMoveLine>("MoveLineIds")
                    .Where(ml => ml.PackageLevelId == null && ml.PackageId == this.PackageId)
                    .ForEach(ml => ml.Delete());
                this.PackageId.GetCollection<StockQuant>("QuantIds").ForEach(quant => {
                    var correspondingMls = this.MoveLineIds.Where(ml => ml.ProductId == quant.ProductId && ml.LotId == quant.LotId);
                    var toDispatch = quant.Quantity;
                    if (correspondingMls.Any()) {
                        correspondingMls.ForEach(ml => {
                            var qty = toDispatch;
                            if (correspondingMls.Count() > 1) {
                                qty = Math.Min(toDispatch, ml.MoveId.ProductQty);
                            }
                            toDispatch -= qty;
                            if (mlUpdateDict.ContainsKey(ml)) {
                                mlUpdateDict[ml] += qty;
                            } else {
                                mlUpdateDict.Add(ml, qty);
                            }
                            if (Math.Abs(toDispatch) < ml.ProductId.UomId.Rounding) {
                                return;
                            }
                        });
                    } else {
                        var correspondingMove = this.MoveIds.FirstOrDefault(m => m.ProductId == quant.ProductId);
                        Env.GetModel("Stock.StockMoveLine").Create(new StockMoveLine {
                            LocationId = this.LocationId,
                            LocationDestId = this.LocationDestId,
                            PickingId = this.PickingId,
                            ProductId = quant.ProductId,
                            Quantity = quant.Quantity,
                            ProductUomId = quant.ProductId.UomId,
                            LotId = quant.LotId,
                            PackageId = this.PackageId,
                            ResultPackageId = this.PackageId,
                            PackageLevelId = this,
                            MoveId = correspondingMove,
                            OwnerId = quant.OwnerId,
                            Picked = true
                        });
                    }
                });
                mlUpdateDict.ForEach(rec => {
                    rec.Key.Quantity = rec.Value;
                    rec.Key.Picked = true;
                });
            } else {
                this.MoveLineIds.ForEach(ml => ml.Delete());
            }
        }
    }

    public void ComputeFreshPack() {
        if (!this.MoveLineIds.Any() || this.MoveLineIds.All(ml => ml.PackageId != null && ml.PackageId == ml.ResultPackageId)) {
            this.IsFreshPackage = false;
        } else {
            this.IsFreshPackage = true;
        }
    }

    public void ComputeState() {
        if (!this.MoveIds.Any() && !this.MoveLineIds.Any()) {
            this.State = "Draft";
        } else if (!this.MoveLineIds.Any() && this.MoveIds.Any(m => m.State != "Done" && m.State != "Cancel")) {
            this.State = "Confirmed";
        } else if (this.MoveLineIds.Any() && !this.MoveLineIds.Any(ml => ml.State == "Done" || ml.State == "Cancel")) {
            if (this.IsFreshPackage) {
                this.State = "New";
            } else if (this.CheckMoveLinesMapQuantPackage(this.PackageId)) {
                this.State = "Assigned";
            } else {
                this.State = "Confirmed";
            }
        } else if (this.MoveLineIds.Any(ml => ml.State == "Done")) {
            this.State = "Done";
        } else if (this.MoveLineIds.Any(ml => ml.State == "Cancel") || this.MoveIds.Any(m => m.State == "Cancel")) {
            this.State = "Cancel";
        } else {
            this.State = "Draft";
        }
    }

    public void ComputeShowLot() {
        if (this.MoveLineIds.Any(ml => ml.ProductId.Tracking != "None")) {
            if (this.PickingId.PickingTypeId.UseExistingLots || this.State == "Done") {
                this.ShowLotsM2o = true;
                this.ShowLotsText = false;
            } else {
                if (this.PickingId.PickingTypeId.UseCreateLots && this.State != "Done") {
                    this.ShowLotsM2o = false;
                    this.ShowLotsText = true;
                } else {
                    this.ShowLotsM2o = false;
                    this.ShowLotsText = false;
                }
            }
        } else {
            this.ShowLotsM2o = false;
            this.ShowLotsText = false;
        }
    }

    public void GenerateMoves() {
        this.PackageId.GetCollection<StockQuant>("QuantIds").ForEach(quant => {
            Env.GetModel("Stock.StockMove").Create(new StockMove {
                PickingId = this.PickingId,
                Name = quant.ProductId.DisplayName,
                ProductId = quant.ProductId,
                ProductUomQty = quant.Quantity,
                ProductUom = quant.ProductId.UomId,
                LocationId = this.LocationId,
                LocationDestId = this.LocationDestId,
                PackageLevelId = this,
                CompanyId = this.CompanyId
            });
        });
    }

    public bool CheckMoveLinesMapQuantPackage(StockQuantPackage package, bool onlyPicked = false) {
        var mls = this.MoveLineIds;
        if (onlyPicked) {
            mls = mls.Where(ml => ml.Picked);
        }
        return package.CheckMoveLinesMapQuant(mls);
    }

    public void ComputeLocationId() {
        if (this.State == "New" || this.IsFreshPackage) {
            this.LocationId = null;
        } else if (this.State != "Done" && this.PackageId != null) {
            this.LocationId = this.PackageId.LocationId;
        } else if (this.State == "Confirmed" && this.MoveIds.Any()) {
            this.LocationId = this.MoveIds.First().LocationId;
        } else if (this.State == "Assigned" || this.State == "Done" && this.MoveLineIds.Any()) {
            this.LocationId = this.MoveLineIds.First().LocationId;
        } else {
            this.LocationId = this.PickingId.LocationId;
        }
    }

    public void ComputeLocationDestId() {
        this.LocationDestId = this.PickingId.LocationDestId;
    }

    public void ActionShowPackageDetails() {
        var view = Env.GetModel("ir.ui.view").GetById("stock.package_level_form_edit_view");
        var action = new Dictionary<string, object> {
            { "name", "Package Content" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "Stock.StockPackageLevel" },
            { "views", new List<object[]> { new object[] { view.Id, "form" } } },
            { "view_id", view.Id },
            { "target", "new" },
            { "res_id", this.Id },
            { "flags", new Dictionary<string, object> { { "mode", "readonly" } } }
        };
        Env.GetActionManager().Run(action);
    }
}
