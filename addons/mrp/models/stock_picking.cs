C#
public partial class Mrp.StockPickingType {

    public void ComputeUseCreateLots()
    {
        if (this.Code == "mrp_operation")
        {
            this.UseCreateLots = true;
        }
    }

    public void ComputeUseExistingLots()
    {
        if (this.Code == "mrp_operation")
        {
            this.UseExistingLots = true;
        }
    }

    public void GetMoCount()
    {
        var mrpPickingTypes = Env.GetRecords<Mrp.StockPickingType>().Where(picking => picking.Code == "mrp_operation");
        var remaining = Env.GetRecords<Mrp.StockPickingType>().Except(mrpPickingTypes);
        remaining.ForEach(record =>
        {
            record.CountMoWaiting = record.CountMoTodo = record.CountMoLate = 0;
        });

        var domains = new Dictionary<string, List<object>>()
        {
            { "CountMoWaiting", new List<object>() { "reservation_state", "=", "waiting" } },
            { "CountMoTodo", new List<object>() { "|", "state", "in", new List<object>() { "confirmed", "draft", "progress", "to_close" }, "is_planned", "=", true } },
            { "CountMoLate", new List<object>() { "date_start", "<", Env.GetDateToday(), "state", "=", "confirmed" } }
        };

        foreach (var keyValuePair in domains)
        {
            var data = Env.GetRecords<Mrp.MrpProduction>().ReadGroup(keyValuePair.Value.Concat(new List<object>() { "state", "not in", new List<object>() { "done", "cancel" }, "picking_type_id", "in", mrpPickingTypes.Select(record => record.Id).ToList() }).ToList(), new List<string>() { "picking_type_id" }, new List<string>() { "__count" });
            var count = data.ToDictionary(record => record[0], record => record[1]);
            mrpPickingTypes.ForEach(record =>
            {
                record[keyValuePair.Key] = count.ContainsKey(record.Id) ? count[record.Id] : 0;
            });
        }
    }

    public Mrp.IrActionsActions GetMrpStockPickingActionPickingType()
    {
        var action = Env.GetXmlId<Mrp.IrActionsActions>("mrp.mrp_production_action_picking_deshboard");
        action.DisplayName = this.DisplayName;
        return action;
    }

    public List<object> GetAggregatedRecordsByDate()
    {
        var productionPickingTypes = Env.GetRecords<Mrp.StockPickingType>().Where(picking => picking.Code == "mrp_operation");
        var otherPickingTypes = Env.GetRecords<Mrp.StockPickingType>().Except(productionPickingTypes);

        var records = otherPickingTypes.GetAggregatedRecordsByDate();
        var mrpRecords = Env.GetRecords<Mrp.MrpProduction>().ReadGroup(new List<object>() { "picking_type_id", "in", productionPickingTypes.Select(record => record.Id).ToList(), "state", "=", "confirmed" }, new List<string>() { "picking_type_id" }, new List<string>() { "date_start:array_agg" });
        mrpRecords = mrpRecords.Select(record => new List<object>() { record[0], record[1], "Confirmed" }).ToList();
        return records.Concat(mrpRecords).ToList();
    }
}
