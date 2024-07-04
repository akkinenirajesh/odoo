C#
public partial class MrpProduction {
    public virtual string Name { get; set; }
    public virtual string Priority { get; set; }
    public virtual int BackorderSequence { get; set; }
    public virtual string Origin { get; set; }
    public virtual Product.Product ProductId { get; set; }
    public virtual Product.TemplateAttribute.Value ProductVariantAttributes { get; set; }
    public virtual string ProductTracking { get; set; }
    public virtual Product.Template ProductTmplId { get; set; }
    public virtual double ProductQty { get; set; }
    public virtual Uom.Uom ProductUomId { get; set; }
    public virtual Stock.Lot LotProducingId { get; set; }
    public virtual double QtyProducing { get; set; }
    public virtual Uom.Uom ProductUomCategoryId { get; set; }
    public virtual double ProductUomQty { get; set; }
    public virtual Stock.Picking.Type PickingTypeId { get; set; }
    public virtual bool UseCreateComponentsLots { get; set; }
    public virtual Stock.Location LocationSrcId { get; set; }
    public virtual Stock.Warehouse WarehouseId { get; set; }
    public virtual Stock.Location LocationDestId { get; set; }
    public virtual Stock.Location LocationFinalId { get; set; }
    public virtual DateTime DateDeadline { get; set; }
    public virtual DateTime DateStart { get; set; }
    public virtual DateTime DateFinished { get; set; }
    public virtual double DurationExpected { get; set; }
    public virtual double Duration { get; set; }
    public virtual Mrp.Bom BomId { get; set; }
    public virtual string State { get; set; }
    public virtual string ReservationState { get; set; }
    public virtual Stock.Move MoveRawIds { get; set; }
    public virtual Stock.Move MoveFinishedIds { get; set; }
    public virtual Stock.Move MoveByproductIds { get; set; }
    public virtual Stock.Move.Line FinishedMoveLineIds { get; set; }
    public virtual Mrp.Workorder WorkorderIds { get; set; }
    public virtual Stock.Move MoveDestIds { get; set; }
    public virtual bool UnreserveVisible { get; set; }
    public virtual bool ReserveVisible { get; set; }
    public virtual Res.Users UserId { get; set; }
    public virtual Res.Company CompanyId { get; set; }
    public virtual double QtyProduced { get; set; }
    public virtual Procurement.Group ProcurementGroupId { get; set; }
    public virtual string ProductDescriptionVariants { get; set; }
    public virtual Stock.Warehouse.Orderpoint OrderpointId { get; set; }
    public virtual bool PropagateCancel { get; set; }
    public virtual DateTime DelayAlertDate { get; set; }
    public virtual string JsonPopover { get; set; }
    public virtual Stock.Scrap ScrapIds { get; set; }
    public virtual int ScrapCount { get; set; }
    public virtual Mrp.Unbuild UnbuildIds { get; set; }
    public virtual int UnbuildCount { get; set; }
    public virtual bool IsLocked { get; set; }
    public virtual bool IsPlanned { get; set; }
    public virtual bool ShowFinalLots { get; set; }
    public virtual Stock.Location ProductionLocationId { get; set; }
    public virtual Stock.Picking PickingIds { get; set; }
    public virtual int DeliveryCount { get; set; }
    public virtual string Consumption { get; set; }
    public virtual int MrpProductionChildCount { get; set; }
    public virtual int MrpProductionSourceCount { get; set; }
    public virtual int MrpProductionBackorderCount { get; set; }
    public virtual bool ShowLock { get; set; }
    public virtual string ComponentsAvailability { get; set; }
    public virtual string ComponentsAvailabilityState { get; set; }
    public virtual double ProductionCapacity { get; set; }
    public virtual bool ShowLotIds { get; set; }
    public virtual bool ForecastedIssue { get; set; }
    public virtual bool ShowAllocation { get; set; }
    public virtual bool AllowWorkorderDependencies { get; set; }
    public virtual bool ShowProduce { get; set; }
    public virtual bool ShowProduceAll { get; set; }
    public virtual bool IsOutdatedBom { get; set; }
    public virtual bool IsDelayed { get; set; }

    public virtual DateTime GetDefaultDateStart() {
        if (Env.Context.ContainsKey("default_DateDeadline")) {
            DateTime dateFinished = Env.Context["default_DateDeadline"];
            DateTime dateStart = dateFinished.AddHours(-1);
            return dateStart;
        }
        return DateTime.Now;
    }

    public virtual DateTime GetDefaultDateFinished() {
        if (Env.Context.ContainsKey("default_DateDeadline")) {
            return Env.Context["default_DateDeadline"];
        }
        DateTime dateStart = DateTime.Now;
        DateTime dateFinished = dateStart.AddHours(1);
        return dateFinished;
    }

    public virtual bool GetDefaultIsLocked() {
        return !Env.User.IsInGroup("Mrp.GroupUnlockedByDefault");
    }

    public virtual void ComputeProductId() {
        if (this.State != "Draft") {
            return;
        }
        if (this.BomId != null && this._origin.BomId != this.BomId) {
            this.ProductId = this.BomId.ProductId;
        }
    }

    public virtual void ComputeBomId() {
        if (this.State != "Draft") {
            return;
        }
        if (this.ProductId != null) {
            // this is a complex domain. It needs to be translated to xml. 
        }
    }

    public virtual void ComputeProductQty() {
        if (this.State != "Draft") {
            return;
        }
        if (this.BomId != null && this._origin.BomId != this.BomId) {
            this.ProductQty = this.BomId.ProductQty;
        } else if (this.BomId == null) {
            this.ProductQty = 1.0;
        }
    }

    public virtual void ComputeUomId() {
        if (this.State != "Draft") {
            return;
        }
        if (this.BomId != null && this._origin.BomId != this.BomId) {
            this.ProductUomId = this.BomId.ProductUomId;
        } else if (this.ProductId != null) {
            this.ProductUomId = this.ProductId.UomId;
        }
    }

    public virtual void ComputePickingTypeId() {
        // this is complex domain
    }

    public virtual void ComputeLocations() {
        if (this.PickingTypeId == null) {
            // this is complex domain
        }
    }

    public virtual void ComputeComponentsAvailability() {
        if (this.State == "Cancel" || this.State == "Done" || this.State == "Draft") {
            return;
        }
        // this is complex logic
    }

    public virtual void ComputeProductionCapacity() {
        if (this.MoveRawIds == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeDateDeadline() {
        if (this.MoveFinishedIds == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeDurationExpected() {
        if (this.WorkorderIds == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeDuration() {
        if (this.WorkorderIds == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeIsPlanned() {
        if (this.WorkorderIds != null) {
            this.IsPlanned = this.WorkorderIds.Any(wo => wo.DateStart != null && wo.DateFinished != null);
        }
    }

    public virtual void ComputeDelayAlertDate() {
        // complex logic
    }

    public virtual void ComputeJsonPopover() {
        // complex logic
    }

    public virtual void ComputePickingIds() {
        if (this.ProcurementGroupId == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeProductUomQty() {
        if (this.ProductId.UomId != this.ProductUomId) {
            this.ProductUomQty = this.ProductUomId.ComputeQuantity(this.ProductQty, this.ProductId.UomId);
        } else {
            this.ProductUomQty = this.ProductQty;
        }
    }

    public virtual void ComputeProductionLocation() {
        if (this.CompanyId == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeShowLots() {
        this.ShowFinalLots = this.ProductId.Tracking != "None";
    }

    public virtual void InverseLines() {
    }

    public virtual void ComputeLines() {
        if (this.MoveFinishedIds == null) {
            return;
        }
        this.FinishedMoveLineIds = this.MoveFinishedIds.SelectMany(m => m.MoveLineIds).ToList();
    }

    public virtual void ComputeState() {
        if (this.State == null) {
            this.State = "Draft";
        } else if (this.State == "Cancel" || this.MoveFinishedIds.All(move => move.State == "Cancel")) {
            this.State = "Cancel";
        } else if (this.State == "Done" || this.MoveRawIds.All(move => move.State == "Cancel" || move.State == "Done") && this.MoveFinishedIds.All(move => move.State == "Cancel" || move.State == "Done")) {
            this.State = "Done";
        } else if (this.WorkorderIds != null && this.WorkorderIds.All(wo => wo.State == "Done" || wo.State == "Cancel")) {
            this.State = "ToClose";
        } else if (this.WorkorderIds == null && this.QtyProducing >= this.ProductQty) {
            this.State = "ToClose";
        } else if (this.WorkorderIds != null && this.WorkorderIds.Any(wo => wo.State == "Progress" || wo.State == "Done")) {
            this.State = "Progress";
        } else if (this.ProductUomId != null && this.QtyProducing > 0) {
            this.State = "Progress";
        } else if (this.MoveRawIds.Any(move => move.Picked)) {
            this.State = "Progress";
        }
    }

    public virtual void ComputeWorkorderIds() {
        if (this.State != "Draft") {
            return;
        }
        if (this.ProductId != null && this.ProductQty > 0) {
            // complex logic
        }
    }

    public virtual void ComputeReservationState() {
        if (this.State == "Draft" || this.State == "Done" || this.State == "Cancel") {
            return;
        }
        // complex logic
    }

    public virtual void ComputeUnreserveVisible() {
        bool alreadyReserved = this.State != "Done" && this.State != "Cancel" && this.MoveRawIds.Any(m => m.MoveLineIds != null);
        bool anyQuantityDone = this.MoveRawIds.Any(move => move.Picked);
        this.UnreserveVisible = !anyQuantityDone && alreadyReserved;
        this.ReserveVisible = this.State == "Confirmed" || this.State == "Progress" || this.State == "ToClose" && this.MoveRawIds.Any(move => move.ProductUomQty != 0 && move.State == "Confirmed" || move.State == "PartiallyAvailable");
    }

    public virtual void GetProducedQty() {
        // complex logic
    }

    public virtual void ComputeScrapMoveCount() {
        // complex logic
    }

    public virtual void ComputeUnbuildCount() {
        this.UnbuildCount = this.UnbuildIds.Count();
    }

    public virtual void ComputeMoveByproductIds() {
        this.MoveByproductIds = this.MoveFinishedIds.Where(m => m.ProductId != this.ProductId).ToList();
    }

    public virtual void SetMoveByproductIds() {
        // complex logic
    }

    public virtual void ComputeShowLock() {
        // complex logic
    }

    public virtual void ComputeShowLotIds() {
        this.ShowLotIds = this.State != "Draft" && this.MoveRawIds.Any(m => m.ProductId.Tracking != "None");
    }

    public virtual void ComputeShowAllocation() {
        if (!Env.User.IsInGroup("Mrp.GroupMrpReceptionReport")) {
            return;
        }
        if (this.PickingTypeId == null) {
            return;
        }
        // complex logic
    }

    public virtual void ComputeForecastedIssue() {
        if (this.ProductId == null) {
            return;
        }
        // complex logic
    }

    public virtual void SearchDelayAlertDate(string operator_, object value) {
        // complex logic
    }

    public virtual void ComputeDateFinished() {
        if (this.DateStart == null || this.IsPlanned || this.State == "Done") {
            return;
        }
        // complex logic
    }

    public virtual void ComputeMoveRawIds() {
        if (this.State != "Draft" || Env.Context.ContainsKey("skip_ComputeMoveRawIds")) {
            return;
        }
        if (this.BomId != null && this.ProductId != null && this.ProductQty > 0) {
            // complex logic
        }
    }

    public virtual void ComputeMoveFinishedIds() {
        if (this.State != "Draft") {
            return;
        }
        // complex logic
    }

    public virtual void ComputeShowProduce() {
        bool stateOk = this.State == "Confirmed" || this.State == "Progress" || this.State == "ToClose";
        bool qtyNoneOrAll = this.QtyProducing == 0 || this.QtyProducing == this.ProductQty;
        this.ShowProduceAll = stateOk && qtyNoneOrAll;
        this.ShowProduce = stateOk && !qtyNoneOrAll;
    }

    public virtual void SearchIsDelayed(string operator_, object value) {
        // complex logic
    }

    public virtual void ComputeIsDelayed() {
        this.IsDelayed = this.State == "Confirmed" || this.State == "Progress" || this.State == "ToClose" && this.DateDeadline != null && this.DateDeadline < DateTime.Now || this.DateDeadline < this.DateFinished;
    }

    public virtual void OnchangeProducing() {
        this.SetQtyProducing(false);
    }

    public virtual void OnchangeLotProducing() {
        // complex logic
    }

    public virtual void OnchangeProductId() {
        // complex logic
    }

    public virtual void _checkByproducts() {
        // complex logic
    }

    public virtual void Write(object vals) {
        // complex logic
    }

    public virtual void Create(object vals) {
        if (vals.ContainsKey("Name") && (vals["Name"] == null || vals["Name"] == "New")) {
            if (!vals.ContainsKey("PickingTypeId")) {
                vals["PickingTypeId"] = this.GetDefaultPickingTypeId(vals.ContainsKey("CompanyId") ? vals["CompanyId"] : this.CompanyId.Id);
            }
            vals["Name"] = Env.Get("Stock.Picking.Type").Browse(vals["PickingTypeId"]).SequenceId.NextByid();
        }
        if (!vals.ContainsKey("ProcurementGroupId")) {
            vals["ProcurementGroupId"] = this.PrepareProcurementGroupVals(vals).Id;
        }
        // complex logic
    }

    public virtual void Unlink() {
        this.ActionCancel();
        // complex logic
    }

    public virtual object CopyData(object default_) {
        // complex logic
    }

    public virtual void Copy(object default_) {
        // complex logic
    }

    public virtual object ActionGenerateBom() {
        // complex logic
    }

    public virtual object ActionViewMoDelivery() {
        // complex logic
    }

    public virtual void ActionToggleIsLocked() {
        this.IsLocked = !this.IsLocked;
    }

    public virtual object ActionProductForecastReport() {
        // complex logic
    }

    public virtual void ActionUpdateBom() {
        // complex logic
    }

    public virtual void ActionViewMrpProductionChilds() {
        // complex logic
    }

    public virtual void ActionViewMrpProductionSources() {
        // complex logic
    }

    public virtual void ActionViewMrpProductionBackorders() {
        // complex logic
    }

    public virtual void _prepareStockLotValues() {
        // complex logic
    }

    public virtual void ActionGenerateSerial() {
        // complex logic
    }

    public virtual void ActionConfirm() {
        // complex logic
    }

    public virtual void _linkWorkordersAndMoves() {
        if (this.WorkorderIds == null) {
            return;
        }
        // complex logic
    }

    public virtual void ActionAssign() {
        this.MoveRawIds.ActionAssign();
    }

    public virtual void ButtonPlan() {
        // complex logic
    }

    public virtual void _planWorkorders(bool replan = false) {
        if (this.WorkorderIds == null) {
            this.IsPlanned = true;
            return;
        }
        this._linkWorkordersAndMoves();
        // complex logic
    }

    public virtual void ButtonUnplan() {
        // complex logic
    }

    public virtual void _getConsumptionIssues() {
        // complex logic
    }

    public virtual object _actionGenerateConsumptionWizard(object consumptionIssues) {
        // complex logic
    }

    public virtual void _getQuantityProducedIssues() {
        // complex logic
    }

    public virtual object _actionGenerateBackorderWizard(object quantityIssues) {
        // complex logic
    }

    public virtual void ActionCancel() {
        this._actionCancel();
    }

    public virtual void _actionCancel() {
        // complex logic
    }

    public virtual void _getDocumentIterateKey(Stock.Move moveRawId) {
        // complex logic
    }

    public virtual void _calPrice(object consumedMoves) {
    }

    public virtual void _postInventory(bool cancelBackorder = false) {
        // complex logic
    }

    public virtual string _getNameBackorder(string name, int sequence) {
        // complex logic
    }

    public virtual object _getBackorderMoVals() {
        // complex logic
    }

    public virtual object _splitProductions(object amounts = null, bool cancelRemainingQty = false, bool setConsumedQty = false) {
        // complex logic
    }

    public virtual void _actionConfirmMoBackorders() {
        this.WorkorderIds.ActionConfirm();
    }

    public virtual void ButtonMarkDone() {
        // complex logic
    }

    public virtual void PreButtonMarkDone() {
        this._buttonMarkDoneSanityChecks();
        // complex logic
    }

    public virtual void _buttonMarkDoneSanityChecks() {
        // complex logic
    }

    public virtual bool _autoProductionChecks() {
        // complex logic
    }

    public virtual void DoUnreserve() {
        // complex logic
    }

    public virtual object ButtonScrap() {
        // complex logic
    }

    public virtual object ActionSeeMoveScrap() {
        // complex logic
    }

    public virtual object ActionViewReceptionReport() {
        // complex logic
    }

    public virtual object ActionViewMrpProductionUnbuilds() {
        // complex logic
    }

    public virtual object GetEmptyListHelp(string helpMessage) {
        // complex logic