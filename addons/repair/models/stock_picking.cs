csharp
public partial class StockPickingType {

    public void ComputeCountRepair(Env env, StockPickingType this)
    {
        var repairPickingTypes = this.Where(picking => picking.Code == "repair_operation");

        this.CountRepairReady = false;
        this.CountRepairConfirmed = false;
        this.CountRepairUnderRepair = false;

        if (!repairPickingTypes.Any())
        {
            return;
        }

        var pickingTypes = env.RepairOrder.ReadGroup(
            new List<object>
            {
                new[] { "PickingTypeId", "in", repairPickingTypes.Select(picking => picking.Id).ToArray() },
                new[] { "State", "in", new[] { "confirmed", "under_repair" } }
            },
            new List<object>
            {
                "PickingTypeId",
                "IsPartsAvailable",
                "State"
            },
            new List<object>
            {
                new[] { "id", "count" }
            });

        var counts = new Dictionary<int, Dictionary<string, int>>();
        foreach (var pt in pickingTypes)
        {
            var ptCount = counts.GetValueOrDefault(pt.PickingTypeId);
            if (ptCount == null)
            {
                ptCount = new Dictionary<string, int>();
                counts.Add(pt.PickingTypeId, ptCount);
            }

            if (pt.IsPartsAvailable)
            {
                if (!ptCount.ContainsKey("ready"))
                {
                    ptCount.Add("ready", 0);
                }
                ptCount["ready"] += pt.IdCount;
            }

            if (!ptCount.ContainsKey(pt.State))
            {
                ptCount.Add(pt.State, 0);
            }
            ptCount[pt.State] += pt.IdCount;
        }

        foreach (var pt in repairPickingTypes)
        {
            if (!counts.ContainsKey(pt.Id))
            {
                continue;
            }
            pt.CountRepairReady = counts[pt.Id].GetValueOrDefault("ready");
            pt.CountRepairConfirmed = counts[pt.Id].GetValueOrDefault("confirmed");
            pt.CountRepairUnderRepair = counts[pt.Id].GetValueOrDefault("under_repair");
        }
    }

    public void ComputeIsRepairable(Env env, StockPickingType this)
    {
        foreach (var pickingType in this)
        {
            if (!pickingType.ReturnTypeOfIds.Any())
            {
                pickingType.IsRepairable = false;
            }
        }
    }

    public void ComputeDefaultRemoveLocationDestId(Env env, StockPickingType this)
    {
        var repairPickingType = this.Where(pt => pt.Code == "repair_operation");
        var companyIds = repairPickingType.Select(pt => pt.CompanyId).ToList();
        companyIds.Add(0);
        var scrapLocations = env.StockLocation.ReadGroup(
            new List<object>
            {
                new[] { "ScrapLocation", "=", true },
                new[] { "CompanyId", "in", companyIds }
            },
            new List<object>
            {
                "CompanyId"
            },
            new List<object>
            {
                new[] { "id", "min" }
            });
        var scrapLocationsDict = scrapLocations.ToDictionary(l => l.CompanyId, l => l.Id);
        foreach (var pickingType in repairPickingType)
        {
            pickingType.DefaultRemoveLocationDestId = scrapLocationsDict.GetValueOrDefault(pickingType.CompanyId);
        }
    }

    public void ComputeDefaultRecycleLocationDestId(Env env, StockPickingType this)
    {
        foreach (var pickingType in this)
        {
            if (pickingType.Code == "repair_operation")
            {
                var stockLocation = pickingType.WarehouseId.LotStockId;
                pickingType.DefaultRecycleLocationDestId = stockLocation.Id;
            }
        }
    }

    public void GetRepairStockPickingActionPickingType(Env env, StockPickingType this)
    {
        var action = env.IrActionsActions.ForXmlId("repair.action_picking_repair");
        if (this != null)
        {
            action.DisplayName = this.DisplayName;
        }
        // return action;
    }

    public List<object> GetAggregatedRecordsByDate(Env env, StockPickingType this)
    {
        var repairPickingTypes = this.Where(picking => picking.Code == "repair_operation");
        var otherPickingTypes = this.Except(repairPickingTypes);

        var records = otherPickingTypes.GetAggregatedRecordsByDate(env);
        var repairRecords = env.RepairOrder.ReadGroup(
            new List<object>
            {
                new[] { "PickingTypeId", "in", repairPickingTypes.Select(picking => picking.Id).ToArray() },
                new[] { "State", "=", "confirmed" }
            },
            new List<object>
            {
                "PickingTypeId"
            },
            new List<object>
            {
                new[] { "ScheduleDate", "array_agg" }
            });
        repairRecords = repairRecords.Select(r => new object[] { r.PickingTypeId, r.ScheduleDate, "Confirmed" }).ToList();
        return records.Concat(repairRecords).ToList();
    }
}

public partial class StockPicking {

    public void ComputeIsRepairable(Env env, StockPicking this)
    {
        foreach (var picking in this)
        {
            picking.IsRepairable = picking.PickingTypeId.IsRepairable && picking.ReturnId != null;
        }
    }

    public void ComputeNbrRepairs(Env env, StockPicking this)
    {
        foreach (var picking in this)
        {
            picking.NbrRepairs = picking.RepairIds.Count();
        }
    }

    public void ActionRepairReturn(Env env, StockPicking this)
    {
        if (this != null)
        {
            var ctx = env.Context.Copy();
            ctx.Add("default_location_id", this.LocationDestId.Id);
            ctx.Add("default_picking_id", this.Id);
            ctx.Add("default_picking_type_id", this.PickingTypeId.WarehouseId.RepairTypeId.Id);
            ctx.Add("default_partner_id", this.PartnerId != null ? this.PartnerId.Id : 0);
            // return new Action
            // {
            //     Name = "Create Repair",
            //     Type = ActionType.ActWindow,
            //     ViewMode = ViewMode.Form,
            //     ResModel = "repair.order",
            //     ViewId = env.Ref("repair.view_repair_order_form").Id,
            //     Context = ctx
            // };
        }
    }

    public void ActionViewRepairs(Env env, StockPicking this)
    {
        if (this.RepairIds.Any())
        {
            var action = new Action
            {
                ResModel = "repair.order",
                Type = ActionType.ActWindow
            };
            if (this.RepairIds.Count() == 1)
            {
                action.ViewMode = ViewMode.Form;
                action.ResId = this.RepairIds.First().Id;
            }
            else
            {
                action.Name = "Repair Orders";
                action.ViewMode = ViewMode.Tree | ViewMode.Form;
                action.Domain = new[] { new[] { "id", "in", this.RepairIds.Select(r => r.Id).ToArray() } };
            }
            // return action;
        }
    }
}
