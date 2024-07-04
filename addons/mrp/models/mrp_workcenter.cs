csharp
public partial class MrpWorkcenter 
{
    public void Unblock()
    {
        if (this.WorkingState != "blocked")
        {
            throw new Exception("It has already been unblocked.");
        }
        var times = Env.Get<MrpWorkcenterProductivity>().Search(x => x.WorkcenterId == this && x.DateEnd == null);
        times.Write(x => x.DateEnd = DateTime.Now);
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("CompanyId"))
        {
            Env.Get<ResourceMixin>().Browse(ResourceId).Write(x => x.CompanyId = (int)vals["CompanyId"]);
        }
        base.Write(vals);
    }

    public void ActionShowOperations()
    {
        var action = Env.Get<IrActionsActions>()._ForXmlId("mrp.mrp_routing_action");
        action["domain"] = new[] { new Dictionary<string, object>() { { "WorkcenterId", this.Id } } };
        action["context"] = new Dictionary<string, object>() { { "default_workcenter_id", this.Id } };
        //return action;
    }

    public void ActionWorkOrder()
    {
        var action = Env.Get<IrActionsActions>()._ForXmlId("mrp.action_work_orders");
        //return action;
    }

    public void ActionArchive()
    {
        var res = base.ActionArchive();
        var filteredWorkcenters = string.Join(", ", this.Where(x => x.RoutingLineIds.Count > 0).Select(x => x.Name).ToArray());
        if (!string.IsNullOrEmpty(filteredWorkcenters))
        {
            //return new Dictionary<string, object>()
            //{
            //    { "type", "ir.actions.client" },
            //    { "tag", "display_notification" },
            //    { "params", new Dictionary<string, object>()
            //    {
            //        { "title", "Note that archived work center(s): '" + filteredWorkcenters + "' is/are still linked to active Bill of Materials, which means that operations can still be planned on it/them. " +
            //               "To prevent this, deletion of the work center is recommended instead." },
            //        { "type", "warning" },
            //        { "sticky", true },
            //        { "next", new Dictionary<string, object>() { { "type", "ir.actions.act_window_close" } } }
            //    } }
            //};
        }
        //return res;
    }

    public double GetCapacity(Product product)
    {
        var productCapacity = this.CapacityIds.Where(x => x.ProductId == product).FirstOrDefault();
        return productCapacity != null ? productCapacity.Capacity : this.DefaultCapacity;
    }

    public double GetExpectedDuration(Product product)
    {
        var capacity = this.CapacityIds.Where(x => x.ProductId == product).FirstOrDefault();
        return capacity != null ? capacity.TimeStart + capacity.TimeStop : this.TimeStart + this.TimeStop;
    }

    public void ComputeHasRoutingLines()
    {
        this.HasRoutingLines = Env.Get<MrpRoutingWorkcenter>().Search(x => x.WorkcenterId == this).Count() > 0;
    }

    public void ComputeWorkorderCount()
    {
        var mrpWorkorder = Env.Get<MrpWorkorder>();
        var result = new Dictionary<int, Dictionary<string, int>>();
        var resultDurationExpected = new Dictionary<int, double>();
        foreach (var wid in this.Select(x => x.Id))
        {
            result.Add(wid, new Dictionary<string, int>());
            resultDurationExpected.Add(wid, 0.0);
        }
        var countData = mrpWorkorder.ReadGroup(
            new[] { new Dictionary<string, object>() { { "WorkcenterId", new List<int>(this.Select(x => x.Id)) }, { "State", new List<string>() { "pending", "waiting", "ready" } }, { "DateStart", new Dictionary<string, object>() { { "<", DateTime.Now.ToString("yyyy-MM-dd") } } } },
            new[] { "WorkcenterId" },
            new[] { "__count" });
        foreach (var workcenter in countData)
        {
            result[workcenter["WorkcenterId"]]["pending"] = (int)workcenter["__count"];
        }
        var res = mrpWorkorder.ReadGroup(
            new[] { new Dictionary<string, object>() { { "WorkcenterId", new List<int>(this.Select(x => x.Id)) } } },
            new[] { "WorkcenterId", "State" },
            new[] { "DurationExpected:sum", "__count" });
        foreach (var workcenter in res)
        {
            result[workcenter["WorkcenterId"]][workcenter["State"]] = (int)workcenter["__count"];
            if (workcenter["State"] == "pending" || workcenter["State"] == "waiting" || workcenter["State"] == "ready" || workcenter["State"] == "progress")
            {
                resultDurationExpected[workcenter["WorkcenterId"]] += (double)workcenter["DurationExpected:sum"];
            }
        }
        foreach (var workcenter in this)
        {
            workcenter.WorkorderCount = result[workcenter.Id].Sum(x => x.Value);
            workcenter.WorkorderPendingCount = result[workcenter.Id].ContainsKey("pending") ? result[workcenter.Id]["pending"] : 0;
            workcenter.WorkcenterLoad = resultDurationExpected[workcenter.Id];
            workcenter.WorkorderReadyCount = result[workcenter.Id].ContainsKey("ready") ? result[workcenter.Id]["ready"] : 0;
            workcenter.WorkorderProgressCount = result[workcenter.Id].ContainsKey("progress") ? result[workcenter.Id]["progress"] : 0;
            workcenter.WorkorderLateCount = countData.Where(x => x["WorkcenterId"] == workcenter.Id).Select(x => x["__count"]).FirstOrDefault();
        }
    }

    public void ComputeWorkingState()
    {
        foreach (var workcenter in this)
        {
            var timeLog = Env.Get<MrpWorkcenterProductivity>().Search(x => x.WorkcenterId == workcenter && x.DateEnd == null).FirstOrDefault();
            if (timeLog == null)
            {
                workcenter.WorkingState = "normal";
            }
            else if (timeLog.LossType == "productive" || timeLog.LossType == "performance")
            {
                workcenter.WorkingState = "done";
            }
            else
            {
                workcenter.WorkingState = "blocked";
            }
        }
    }

    public void ComputeBlockedTime()
    {
        var data = Env.Get<MrpWorkcenterProductivity>().ReadGroup(
            new[] { new Dictionary<string, object>() { { "DateStart", new Dictionary<string, object>() { { ">=", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") } } }, { "WorkcenterId", new List<int>(this.Select(x => x.Id)) }, { "DateEnd", new Dictionary<string, object>() { { "!=", null } } }, { "LossType", new List<string>() { "productive" } } } },
            new[] { "WorkcenterId" },
            new[] { "Duration:sum" });
        var countData = new Dictionary<int, double>();
        foreach (var workcenter in data)
        {
            countData[workcenter["WorkcenterId"]] = (double)workcenter["Duration:sum"];
        }
        foreach (var workcenter in this)
        {
            workcenter.BlockedTime = countData.ContainsKey(workcenter.Id) ? countData[workcenter.Id] / 60.0 : 0.0;
        }
    }

    public void ComputeProductiveTime()
    {
        var data = Env.Get<MrpWorkcenterProductivity>().ReadGroup(
            new[] { new Dictionary<string, object>() { { "DateStart", new Dictionary<string, object>() { { ">=", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") } } }, { "WorkcenterId", new List<int>(this.Select(x => x.Id)) }, { "DateEnd", new Dictionary<string, object>() { { "!=", null } } }, { "LossType", "productive" } } },
            new[] { "WorkcenterId" },
            new[] { "Duration:sum" });
        var countData = new Dictionary<int, double>();
        foreach (var workcenter in data)
        {
            countData[workcenter["WorkcenterId"]] = (double)workcenter["Duration:sum"];
        }
        foreach (var workcenter in this)
        {
            workcenter.ProductiveTime = countData.ContainsKey(workcenter.Id) ? countData[workcenter.Id] / 60.0 : 0.0;
        }
    }

    public void ComputeOee()
    {
        foreach (var order in this)
        {
            order.Oee = order.ProductiveTime > 0 ? Math.Round((order.ProductiveTime * 100.0 / (order.ProductiveTime + order.BlockedTime)), 2) : 0.0;
        }
    }

    public void ComputePerformance()
    {
        var woData = Env.Get<MrpWorkorder>().ReadGroup(
            new[] { new Dictionary<string, object>() { { "DateStart", new Dictionary<string, object>() { { ">=", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") } } }, { "WorkcenterId", new List<int>(this.Select(x => x.Id)) }, { "State", "done" } } },
            new[] { "WorkcenterId" },
            new[] { "DurationExpected:sum", "Duration:sum" });
        var durationExpected = new Dictionary<int, double>();
        var duration = new Dictionary<int, double>();
        foreach (var workcenter in woData)
        {
            durationExpected[workcenter["WorkcenterId"]] = (double)workcenter["DurationExpected:sum"];
            duration[workcenter["WorkcenterId"]] = (double)workcenter["Duration:sum"];
        }
        foreach (var workcenter in this)
        {
            workcenter.Performance = duration.ContainsKey(workcenter.Id) && duration[workcenter.Id] > 0 ? Math.Round((100 * durationExpected.ContainsKey(workcenter.Id) ? durationExpected[workcenter.Id] : 0.0) / duration[workcenter.Id], 2) : 0.0;
        }
    }

    public void _CheckCapacity()
    {
        if (this.Where(x => x.DefaultCapacity <= 0.0).Any())
        {
            throw new Exception("The capacity must be strictly positive.");
        }
    }

    public void _CheckAlternativeWorkcenter()
    {
        foreach (var workcenter in this)
        {
            if (workcenter.AlternativeWorkcenterIds.Contains(workcenter))
            {
                throw new Exception("Workcenter " + workcenter.Name + " cannot be an alternative of itself.");
            }
        }
    }

    public List<Tuple<DateTime, DateTime>> _GetUnavailabilityIntervals(DateTime startDateTime, DateTime endDateTime)
    {
        var unavailabilityRessources = Env.Get<ResourceMixin>().Browse(ResourceId)._GetUnavailableIntervals(startDateTime, endDateTime);
        return this.Select(x => new Tuple<DateTime, DateTime>(unavailabilityRessources.ContainsKey(x.ResourceId) ? unavailabilityRessources[x.ResourceId] : new List<DateTime>()[0], unavailabilityRessources.ContainsKey(x.ResourceId) ? unavailabilityRessources[x.ResourceId] : new List<DateTime>()[0])).ToList();
    }

    public Tuple<DateTime, DateTime> _GetFirstAvailableSlot(DateTime startDateTime, double duration, bool backward = false, List<MrpWorkcenterProductivity> leavesToIgnore = null)
    {
        if (this.Count() != 1)
        {
            throw new Exception("This method should be called on a single object.");
        }
        var resource = Env.Get<ResourceMixin>().Browse(ResourceId);
        var availableIntervalsDomain = new List<Dictionary<string, object>>() { new Dictionary<string, object>() { { "TimeType", new List<string>() { "other", "leave" } } } };
        var workorderIntervalsDomain = new List<Dictionary<string, object>>() { new Dictionary<string, object>() { { "TimeType", "other" } } };
        if (leavesToIgnore != null)
        {
            availableIntervalsDomain.Add(new Dictionary<string, object>() { { "Id", new List<int>() { leavesToIgnore.Select(x => x.Id).ToArray() } } });
            workorderIntervalsDomain.Add(new Dictionary<string, object>() { { "Id", new List<int>() { leavesToIgnore.Select(x => x.Id).ToArray() } } });
        }
        var getAvailableIntervals = (Func<DateTime, DateTime, List<int>, List<Tuple<DateTime, DateTime, object>>>)
            (s, e, r) => Env.Get<ResourceCalendar>().Browse(resource.ResourceCalendarId)._WorkIntervalsBatch(availableIntervalsDomain, r, resource.ResourceCalendarId.Tz);
        var getWorkorderIntervals = (Func<DateTime, DateTime, List<int>, List<Tuple<DateTime, DateTime, object>>>)
            (s, e, r) => Env.Get<ResourceCalendar>().Browse(resource.ResourceCalendarId)._LeaveIntervalsBatch(workorderIntervalsDomain, r, resource.ResourceCalendarId.Tz);
        var remaining = duration;
        var startInterval = startDateTime;
        var now = DateTime.Now;
        var delta = backward ? new TimeSpan(-14, 0, 0, 0) : new TimeSpan(14, 0, 0, 0);
        for (int n = 0; n < 50; n++)
        {
            var dateStart = startDateTime.Add(delta.Multiply(n));
            var dateStop = dateStart.Add(delta);
            if (backward)
            {
                dateStop = dateStart;
                dateStart = dateStop.Add(delta);
            }
            var availableIntervals = getAvailableIntervals(dateStart, dateStop, new List<int>() { resource.Id })[resource.Id];
            var workorderIntervals = getWorkorderIntervals(dateStart, dateStop, new List<int>() { resource.Id })[resource.Id];
            if (backward)
            {
                availableIntervals = availableIntervals.Reverse().ToList();
            }
            foreach (var interval in availableIntervals)
            {
                for (int _i = 0; _i < 2; _i++)
                {
                    var intervalMinutes = (interval.Item2 - interval.Item1).TotalSeconds / 60;
                    if (remaining == duration)
                    {
                        startInterval = interval.Item1;
                    }
                    if (Env.Get<Intervals>()._And(new List<Tuple<DateTime, DateTime, object>>() { new Tuple<DateTime, DateTime, object>(startInterval, interval.Item1.Add(new TimeSpan(0, 0, (int)Math.Min(remaining, intervalMinutes))), interval.Item3) }, workorderIntervals).Count() > 0)
                    {
                        remaining = duration;
                    }
                    else if (Math.Round(intervalMinutes, 3) >= Math.Round(remaining, 3))
                    {
                        if (backward)
                        {
                            return new Tuple<DateTime, DateTime>(interval.Item2.Add(new TimeSpan(0, 0, -(int)remaining)), startInterval);
                        }
                        return new Tuple<DateTime, DateTime>(startInterval, interval.Item1.Add(new TimeSpan(0, 0, (int)remaining)));
                    }
                    else
                    {
                        remaining -= intervalMinutes;
                        break;
                    }
                }
            }
            if (backward && dateStart <= now)
            {
                break;
            }
        }
        return new Tuple<DateTime, DateTime>(DateTime.MinValue, DateTime.MinValue);
    }

    public void _ComputeDuration()
    {
        foreach (var blocktime in this)
        {
            blocktime.Duration = blocktime.DateStart != null && blocktime.DateEnd != null ?
                blocktime.LossId._ConvertToDuration(blocktime.DateStart.AddMilliseconds(-blocktime.DateStart.Millisecond), blocktime.DateEnd.AddMilliseconds(-blocktime.DateEnd.Millisecond), blocktime.WorkcenterId) : 0.0;
        }
    }

    public void _DurationChanged()
    {
        this.DateStart = this.DateEnd.AddMinutes(-this.Duration);
        this._LossTypeChange();
    }

    public void _DateStartChanged()
    {
        this.DateEnd = this.DateStart.AddMinutes(this.Duration);
        this._LossTypeChange();
    }

    public void _DateEndChanged()
    {
        this.DateStart = this.DateEnd.AddMinutes(-this.Duration);
        this._LossTypeChange();
    }

    public void _LossTypeChange()
    {
        if (this.WorkorderId != null && this.WorkorderId.Duration > this.WorkorderId.DurationExpected)
        {
            this.LossId = Env.Get<MrpWorkcenterProductivityLoss>().Search(x => x.LossType == "performance").FirstOrDefault().Id;
        }
        else
        {
            this.LossId = Env.Get<MrpWorkcenterProductivityLoss>().Search(x => x.LossType == "availability").FirstOrDefault().Id;
        }
    }

    public void ButtonBlock()
    {
        this.WorkcenterId.OrderIds.EndAll();
    }

    public void _Close()
    {
        var underperformanceTimers = new List<MrpWorkcenterProductivity>();
        foreach (var timer in this)
        {
            var wo = timer.WorkorderId;
            timer.Write(x => x.DateEnd = DateTime.Now);
            if (wo.Duration > wo.DurationExpected)
            {
                var productiveDateEnd = timer.DateEnd.AddMinutes(-(wo.Duration - wo.DurationExpected));
                if (productiveDateEnd <= timer.DateStart)
                {
                    underperformanceTimers.Add(timer);
                }
                else
                {
                    underperformanceTimers.Add(timer.Copy(x => x.DateStart = productiveDateEnd));
                    timer.Write(x => x.DateEnd = productiveDateEnd);
                }
            }
        }
        if (underperformanceTimers.Count > 0)
        {
            var underperformanceType = Env.Get<MrpWorkcenterProductivityLoss>().Search(x => x.LossType == "performance").FirstOrDefault();
            if (underperformanceType == null)
            {
                throw new Exception("You need to define at least one unactive productivity loss in the category 'Performance'. Create one from the Manufacturing app, menu: Configuration / Productivity Losses.");
            }
            underperformanceTimers.ForEach(x => x.Write(y => y.LossId = underperformanceType.Id));
        }
    }
}
