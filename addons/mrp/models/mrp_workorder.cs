csharp
public partial class MrpWorkorder
{
    // all the model methods are written here.
    public void ComputeBarcode()
    {
        this.Barcode = $"{this.ProductionId.Name}/{this.Id}";
    }

    public void ComputeState()
    {
        if (this.State == "pending")
        {
            if (this.BlockedByWorkorderIds.All(wo => wo.State == "done" || wo.State == "cancel"))
            {
                this.State = this.ProductionAvailability == "assigned" ? "ready" : "waiting";
                return;
            }
        }
        if (this.State != "waiting" && this.State != "ready")
        {
            return;
        }
        if (!this.BlockedByWorkorderIds.All(wo => wo.State == "done" || wo.State == "cancel"))
        {
            this.State = "pending";
            return;
        }
        if (this.ProductionAvailability != "waiting" && this.ProductionAvailability != "confirmed" && this.ProductionAvailability != "assigned")
        {
            return;
        }
        if (this.ProductionAvailability == "assigned" && this.State == "waiting")
        {
            this.State = "ready";
        }
        else if (this.ProductionAvailability != "assigned" && this.State == "ready")
        {
            this.State = "waiting";
        }
    }

    public void ComputeQtyProducing()
    {
        this.QtyProducing = this.ProductionId.QtyProducing;
    }

    public void SetQtyProducing()
    {
        if (this.QtyProducing != 0 && this.ProductionId.QtyProducing != this.QtyProducing)
        {
            this.ProductionId.QtyProducing = this.QtyProducing;
            this.ProductionId.SetQtyProducing();
        }
    }

    public void ComputeDates()
    {
        this.DateStart = this.LeaveId.DateFrom;
        this.DateFinished = this.LeaveId.DateTo;
    }

    public void SetDates()
    {
        if (this.LeaveId != null)
        {
            if (this.DateStart == null || this.DateFinished == null)
            {
                throw new Exception("It is not possible to unplan one single Work Order. You should unplan the Manufacturing Order instead in order to unplan all the linked operations.");
            }
            this.LeaveId.Write(new Dictionary<string, object>() {
                {"DateFrom", this.DateStart},
                {"DateTo", this.DateFinished}
            });
        }
        else if (this.DateStart != null)
        {
            this.DateFinished = CalculateDateFinished();
            this.LeaveId = Env.Create<ResourceCalendarLeaves>(new Dictionary<string, object>() {
                {"Name", this.DisplayName},
                {"CalendarId", this.WorkcenterId.ResourceCalendarId.Id},
                {"DateFrom", this.DateStart},
                {"DateTo", this.DateFinished},
                {"ResourceId", this.WorkcenterId.ResourceId.Id},
                {"TimeType", "other"}
            });
        }
    }

    public void ComputeJsonPopover()
    {
        // TODO: Implement logic for computing JsonPopover and ShowJsonPopover based on the model's state and related data
    }

    public void ComputeIsProduced()
    {
        this.IsProduced = false;
        if (this.ProductionId != null && this.ProductionId.ProductUomId != null)
        {
            var rounding = this.ProductionId.ProductUomId.Rounding;
            this.IsProduced = FloatCompare(this.QtyProduced, this.ProductionId.ProductQty, rounding) >= 0;
        }
    }

    public void ComputeDurationExpected()
    {
        if (this.State != "done" && this.State != "cancel")
        {
            this.DurationExpected = GetDurationExpected();
        }
    }

    public void ComputeDuration()
    {
        this.Duration = this.TimeIds.Sum(time => time.Duration);
        this.DurationUnit = Math.Round((decimal)(this.Duration / Math.Max(this.QtyProduced, 1)), 2);
        if (this.DurationExpected != 0)
        {
            this.DurationPercent = Math.Max(-2147483648, Math.Min(2147483647, (int)(100 * (this.DurationExpected - this.Duration) / this.DurationExpected)));
        }
        else
        {
            this.DurationPercent = 0;
        }
    }

    public void SetDuration()
    {
        // TODO: Implement logic for setting Duration based on the model's state and related data
    }

    public void ComputeProgress()
    {
        if (this.State == "done")
        {
            this.Progress = 100;
        }
        else if (this.DurationExpected != 0)
        {
            this.Progress = this.Duration * 100 / this.DurationExpected;
        }
        else
        {
            this.Progress = 0;
        }
    }

    public void ComputeWorkingUsers()
    {
        this.WorkingUserIds = this.TimeIds.Where(time => time.DateEnd == null).OrderBy(time => time.DateStart).Select(time => time.UserId).ToList();
        if (this.WorkingUserIds.Count > 0)
        {
            this.LastWorkingUserId = this.WorkingUserIds.Last();
        }
        else if (this.TimeIds.Count > 0)
        {
            this.LastWorkingUserId = this.TimeIds.Where(time => time.DateEnd != null).OrderByDescending(time => time.DateEnd).LastOrDefault()?.UserId ?? this.TimeIds.Last().UserId;
        }
        else
        {
            this.LastWorkingUserId = null;
        }
        if (this.TimeIds.Any(x => x.UserId.Id == Env.User.Id && x.DateEnd == null && (x.LossType == "productive" || x.LossType == "performance")))
        {
            this.IsUserWorking = true;
        }
        else
        {
            this.IsUserWorking = false;
        }
    }

    public void ComputeScrapMoveCount()
    {
        this.ScrapCount = Env.ReadGroup<StockScrap>(new List<object>() {("WorkorderId", "in", new List<int>() {this.Id})}, new List<object>() {"WorkorderId"}, new List<object>() {"__count"})
            .Select(x => new { WorkorderId = (int)x["WorkorderId"], Count = (int)x["__count"] })
            .FirstOrDefault(x => x.WorkorderId == this.Id)?.Count ?? 0;
    }

    public void OnchangeOperationId()
    {
        if (this.OperationId != null)
        {
            this.Name = this.OperationId.Name;
            this.WorkcenterId = this.OperationId.WorkcenterId.Id;
        }
    }

    public void OnchangeDateStart()
    {
        if (this.DateStart != null && this.WorkcenterId != null)
        {
            this.DateFinished = CalculateDateFinished();
        }
    }

    public void OnchangeDateFinished()
    {
        if (this.DateStart != null && this.DateFinished != null && this.WorkcenterId != null)
        {
            this.DurationExpected = CalculateDurationExpected();
        }
        if (this.DateFinished == null && this.DateStart != null)
        {
            throw new Exception("It is not possible to unplan one single Work Order. You should unplan the Manufacturing Order instead in order to unplan all the linked operations.");
        }
    }

    public void OnchangeFinishedLotId()
    {
        if (this.ProductionId != null)
        {
            // TODO: Implement logic for checking the finished lot ID based on the model's state and related data
        }
    }

    public void Write(Dictionary<string, object> values)
    {
        if (values.ContainsKey("ProductionId") && values.Any(x => (int)x.Value != this.ProductionId.Id))
        {
            throw new Exception("You cannot link this work order to another manufacturing order.");
        }
        if (values.ContainsKey("WorkcenterId"))
        {
            if (this.WorkcenterId.Id != (int)values["WorkcenterId"])
            {
                if (this.State == "progress" || this.State == "done" || this.State == "cancel")
                {
                    throw new Exception("You cannot change the workcenter of a work order that is in progress or done.");
                }
                this.LeaveId.ResourceId = Env.Get<MrpWorkcenter>((int)values["WorkcenterId"]).ResourceId;
                this.DurationExpected = GetDurationExpected();
                if (this.DateStart != null)
                {
                    this.DateFinished = CalculateDateFinished();
                }
            }
        }
        if (values.ContainsKey("DateStart") || values.ContainsKey("DateFinished"))
        {
            var dateStart = (DateTime)values.GetValueOrDefault("DateStart", this.DateStart);
            var dateFinished = (DateTime)values.GetValueOrDefault("DateFinished", this.DateFinished);
            if (dateStart != null && dateFinished != null && dateStart > dateFinished)
            {
                throw new Exception("The planned end date of the work order cannot be prior to the planned start date, please correct this to save the work order.");
            }
            if (!values.ContainsKey("DurationExpected") && !Env.Context.ContainsKey("bypassDurationCalculation"))
            {
                if (values.ContainsKey("DateStart") && values.ContainsKey("DateFinished"))
                {
                    var computedFinishedTime = CalculateDateFinished(dateStart);
                    values["DateFinished"] = computedFinishedTime;
                }
                else if (dateStart != null && dateFinished != null)
                {
                    var computedDuration = CalculateDurationExpected(dateStart, dateFinished);
                    values["DurationExpected"] = computedDuration;
                }
            }
            // Update MO dates if the start date of the first WO or the finished date of the last WO is update.
            if (this == this.ProductionId.WorkorderIds.FirstOrDefault() && values.ContainsKey("DateStart"))
            {
                if (values["DateStart"] != null)
                {
                    this.ProductionId.WithContext(new Dictionary<string, object>() { { "forceDate", true } }).Write(new Dictionary<string, object>() { { "DateStart", (DateTime)values["DateStart"] } });
                }
            }
            if (this == this.ProductionId.WorkorderIds.LastOrDefault() && values.ContainsKey("DateFinished"))
            {
                if (values["DateFinished"] != null)
                {
                    this.ProductionId.WithContext(new Dictionary<string, object>() { { "forceDate", true } }).Write(new Dictionary<string, object>() { { "DateFinished", (DateTime)values["DateFinished"] } });
                }
            }
        }
        // Call base Write method
        base.Write(values);
    }

    public void Create(Dictionary<string, object> values)
    {
        // Call base Create method
        base.Create(values);
        // Auto-confirm manually added workorders.
        // We need to go through _action_confirm for all workorders of the current productions to make sure the links between them are correct.
        if (!Env.Context.ContainsKey("skipConfirm"))
        {
            var toConfirm = this.Where(wo => wo.ProductionId.State == "confirmed" || wo.ProductionId.State == "progress" || wo.ProductionId.State == "to_close");
            toConfirm = toConfirm.ProductionId.WorkorderIds;
            toConfirm.ActionConfirm();
        }
    }

    public void ActionConfirm()
    {
        this.ProductionId.LinkWorkordersAndMoves();
    }

    public List<StockMove> GetByproductMoveToUpdate()
    {
        return this.ProductionId.MoveFinishedIds.Where(x => x.ProductId.Id != this.ProductionId.ProductId.Id && x.State != "done" && x.State != "cancel").ToList();
    }

    public void PlanWorkorder(bool replan = false)
    {
        // Plan workorder after its predecessors
        var dateStart = DateTime.Now;
        foreach (var workorder in this.BlockedByWorkorderIds)
        {
            if (workorder.State == "done" || workorder.State == "cancel")
            {
                continue;
            }
            workorder.PlanWorkorder(replan);
            if (workorder.DateFinished != null && workorder.DateFinished > dateStart)
            {
                dateStart = (DateTime)workorder.DateFinished;
            }
        }
        // Plan only suitable workorders
        if (this.State != "pending" && this.State != "waiting" && this.State != "ready")
        {
            return;
        }
        if (this.LeaveId != null)
        {
            if (replan)
            {
                this.LeaveId.Unlink();
            }
            else
            {
                return;
            }
        }
        // Consider workcenter and alternatives
        var workcenters = this.WorkcenterId | this.WorkcenterId.AlternativeWorkcenterIds;
        var bestDateFinished = DateTime.MaxValue;
        var vals = new Dictionary<string, object>();
        foreach (var workcenter in workcenters)
        {
            if (workcenter.ResourceCalendarId == null)
            {
                throw new Exception($"There is no defined calendar on workcenter {workcenter.Name}.");
            }
            // Compute theoretical duration
            var durationExpected = this.WorkcenterId == workcenter ? this.DurationExpected : GetDurationExpected(alternativeWorkcenter: workcenter);
            var fromDate = workcenter.GetFirstAvailableSlot(dateStart, durationExpected).Item1;
            var toDate = workcenter.GetFirstAvailableSlot(dateStart, durationExpected).Item2;
            // If the workcenter is unavailable, try planning on the next one
            if (fromDate == null)
            {
                continue;
            }
            // Check if this workcenter is better than the previous ones
            if (toDate != null && toDate < bestDateFinished)
            {
                var bestDateStart = fromDate;
                bestDateFinished = toDate;
                var bestWorkcenter = workcenter;
                vals = new Dictionary<string, object>() {
                    {"WorkcenterId", bestWorkcenter.Id},
                    {"DurationExpected", durationExpected},
                };
            }
        }
        // If none of the workcenter are available, raise
        if (bestDateFinished == DateTime.MaxValue)
        {
            throw new Exception("Impossible to plan the workorder. Please check the workcenter availabilities.");
        }
        // Create leave on chosen workcenter calendar
        var leave = Env.Create<ResourceCalendarLeaves>(new Dictionary<string, object>() {
            {"Name", this.DisplayName},
            {"CalendarId", bestWorkcenter.ResourceCalendarId.Id},
            {"DateFrom", bestDateStart},
            {"DateTo", bestDateFinished},
            {"ResourceId", bestWorkcenter.ResourceId.Id},
            {"TimeType", "other"}
        });
        vals["LeaveId"] = leave.Id;
        this.Write(vals);
    }

    public double CalCost()
    {
        var total = 0.0;
        foreach (var wo in this)
        {
            var duration = wo.TimeIds.Sum(time => time.Duration);
            total += (duration / 60.0) * wo.WorkcenterId.CostsHour;
        }
        return total;
    }

    public Dictionary<string, object> GetGanttData(List<object> domain, List<string> groupby, Dictionary<string, object> readSpecification, int? limit = null, int offset = 0, List<string> unavailabilityFields = null, List<string> progressBarFields = null, string startDate = null, string stopDate = null, string scale = null)
    {
        var ganttData = base.GetGanttData(domain, groupby, readSpecification, limit, offset, unavailabilityFields, progressBarFields, startDate, stopDate, scale);
        if (!ganttData["unavailabilities"].ContainsKey("WorkcenterId"))
        {
            var workcenterIds = new HashSet<int>();
            if (groupby != null && groupby.Contains("WorkcenterId"))
            {
                foreach (var group in ganttData["groups"])
                {
                    var resId = group["WorkcenterId"][0] != null ? (int)group["WorkcenterId"][0] : 0;
                    workcenterIds.Add(resId);
                }
            }
            else
            {
                foreach (var record in ganttData["records"])
                {
                    var resId = record.ContainsKey("WorkcenterId") ? (int)record["WorkcenterId"]["id"] : 0;
                    workcenterIds.Add(resId);
                }
            }
            var start = DateTime.Parse(startDate);
            var stop = DateTime.Parse(stopDate);
            ganttData["unavailabilities"]["WorkcenterId"] = GetGanttUnavailability("WorkcenterId", workcenterIds.ToList(), start, stop, scale);
        }
        return ganttData;
    }

    public List<Dictionary<string, object>> GetGanttUnavailability(string field, List<int> resIds, DateTime start, DateTime stop, string scale)
    {
        if (field != "WorkcenterId")
        {
            return base.GetGanttUnavailability(field, resIds, start, stop, scale);
        }
        var workcenters = Env.Get<MrpWorkcenter>(resIds);
        var unavailabilityMapping = workcenters.GetUnavailabilityIntervals(start, stop);
        var result = new Dictionary<int, List<Dictionary<string, object>>>();
        foreach (var workcenter in workcenters)
        {
            result[workcenter.Id] = unavailabilityMapping[workcenter.Id].Select(interval => new Dictionary<string, object>() { { "start", interval.Item1 }, { "stop", interval.Item2 } }).ToList();
        }
        return result.Values.ToList();
    }

    public void ButtonStart()
    {
        if (this.Any(wo => wo.WorkingState == "blocked"))
        {
            throw new Exception("Please unblock the work center to start the work order.");
        }
        foreach (var wo in this)
        {
            if (wo.TimeIds.Any(time => time.UserId.Id == Env.User.Id && time.DateEnd == null))
            {
                continue;
            }
            // As button_start is automatically called in the new view
            if (wo.State == "done" || wo.State == "cancel")
            {
                continue;
            }
            if (wo.ProductTracking == "serial" && wo.QtyProducing == 0)
            {
                wo.QtyProducing = 1.0;
            }
            else if (wo.QtyProducing == 0)
            {
                wo.QtyProducing = wo.QtyRemaining;
            }
            if (ShouldStartTimer())
            {
                Env.Create<MrpWorkcenterProductivity>(wo.PrepareTimelineVals(wo.Duration, DateTime.Now));
            }
            if (wo.ProductionId.State != "progress")
            {
                wo.ProductionId.Write(new Dictionary<string, object>() { {"DateStart", DateTime.Now} });
            }
            if (wo.State == "progress")
            {
                continue;
            }
            var dateStart = DateTime.Now;
            var vals = new Dictionary<string, object>() {
                {"State", "progress"},
                {"DateStart", dateStart},
            };
            if (wo.LeaveId == null)
            {
                var leave = Env.Create<ResourceCalendarLeaves>(new Dictionary<string, object>() {
                    {"Name", wo.DisplayName},
                    {"CalendarId", wo.WorkcenterId.ResourceCalendarId.Id},
                    {"DateFrom", dateStart},
                    {"DateTo", dateStart.AddMinutes(wo.DurationExpected)},
                    {"ResourceId", wo.WorkcenterId.ResourceId.Id},
                    {"TimeType", "other"}
                });
                vals["DateFinished"] = leave.DateTo;
                vals["LeaveId"] = leave.Id;
                wo.Write(vals);
            }
            else
            {
                if (wo.DateStart == null || wo.DateStart > dateStart)
                {
                    vals["DateStart"] = dateStart;
                    vals["DateFinished"] = wo.CalculateDateFinished(dateStart);
                }
                if (wo.DateFinished != null && wo.DateFinished < dateStart)
                {
                    vals["DateFinished"] = dateStart;
                }
                wo.WithContext(new Dictionary<string, object>() { {"bypassDurationCalculation", true} }).Write(vals);
            }
        }
    }

    public void ButtonFinish()
    {
        var dateFinished = DateTime.Now;
        foreach (var workorder in this)
        {
            if (workorder.State == "done" || workorder.State == "cancel")
            {
                continue;
            }
            workorder.EndAll();
            var vals = new Dictionary<string, object>() {
                {"QtyProduced", workorder.QtyProduced ?? workorder.QtyProducing ?? workorder.QtyProduction},
                {"State", "done"},
                {"DateFinished", dateFinished},
                {"CostsHour", workorder.WorkcenterId.CostsHour}
            };
            if (workorder.DateStart == null || dateFinished < workorder.DateStart)
            {
                vals["DateStart"] = dateFinished;
            }
            workorder.WithContext(new Dictionary<string, object>() { {"bypassDurationCalculation", true} }).Write(vals);
        }
    }

    public void EndPrevious(bool doall = false)
    {
        // TDE CLEANME
        var domain = new List<object>() { ("WorkorderId", "in", this.Select(x => x.Id).ToList()), ("DateEnd", "=", null) };
        if (!doall)
        {
            domain.Add(("UserId", "=", Env.User.Id));
        }
        Env.Get<MrpWorkcenterProductivity>(domain, limit: doall ? null : 1)._Close();
    }

    public void EndAll()
    {
        EndPrevious(doall: true);
    }

    public void ButtonPending()
    {
        EndPrevious();
    }

    public void ButtonUnblock()
    {
        this.ForEach(order => order.WorkcenterId.Unblock());
    }

    public void ActionCancel()
    {
        this.LeaveId.Unlink();
        this.EndAll();
        this.Write(new Dictionary<string, object>() { {"State", "cancel"} });
    }

    public void ActionReplan()
    {
        // Replan a work order. It actually replans every "ready" or "pending" work orders of the linked manufacturing orders.
        this.ProductionId.ForEach(production => production.PlanWorkorders(replan: true));
    }

    public void ButtonDone()
    {
        if (this.Any(x => x.State == "done" || x.State == "cancel"))
        {
            throw new Exception("A Manufacturing Order is already done or cancelled.");
        }
        this.EndAll();
        var endDate = DateTime.Now;
        this.Write(new Dictionary<string, object>() {
            {"State", "done"},
            {"DateFinished", endDate},
            {"CostsHour", this.WorkcenterId.CostsHour}
        });
    }

    public Dictionary<string, object> ButtonScrap()
    {
        if (this.Count != 1)
        {
            throw new Exception("Operation can only be performed on one record at a time");
        }
        return new Dictionary<string, object>() {
            {"name", "Scrap Products"},
            {"viewMode", "form"},
            {"resModel", "Stock.Scrap"},
            {"views", new List<object>() { (Env.Get<StockScrap>("stock.stock_scrap_form_view2").Id, "form") }},
            {"type", "ir.actions.act_window"},
            {"context", new Dictionary<string, object>() {
                {"defaultCompanyId", this.ProductionId.CompanyId.Id},
                {"defaultWorkorderId", this.Id},
                {"defaultProductionId", this.ProductionId.Id},
                {"productIds", (this.ProductionId.MoveRawIds.Where(x => x.State != "done" && x.State != "cancel").ToList() | this.ProductionId.MoveFinishedIds.Where(x => x.State == "done").ToList()).Select(x => x.ProductId.Id).ToList()}
            }},
            {"target", "new"}
        };
    }

    public Dictionary<string, object> ActionSeeMoveScrap()
    {
        if (this.Count != 1)
        {
            throw new Exception("Operation can only be performed on one record at a time");
        }
        var action = Env.Get<IrActions>("stock.action_stock_scrap");
        action["domain"] = new List<object>() { ("WorkorderId", "=", this.Id) };
        return action;
    }

    public Dictionary<string, object> ActionOpenWizard()
    {
        if (this.Count != 1)
        {
            throw new Exception("Operation can only be performed on one record at a time");
        }
        var action = Env.Get<IrActions>("mrp.mrp_workorder_mrp_production_form");
        action["resId"] = this.Id;
        return action;
    }

    public void ComputeQtyRemaining()
    {
        foreach (var wo in this)
        {
            if (wo.ProductionId.ProductUomId != null)
            {
                wo.QtyRemaining = Math.Max(FloatRound(wo.QtyProduction - wo.QtyReportedFromPreviousWo - wo.QtyProduced, wo.ProductionId.ProductUomId.Rounding), 0);
            }
            else
            {
                wo.QtyRemaining = 0;
            }
        }
    }

    private double GetDurationExpected(Mr