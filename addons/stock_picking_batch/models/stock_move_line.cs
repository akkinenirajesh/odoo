csharp
public partial class StockMoveLine {

    public Dictionary<string, object> ActionOpenAddToWave() {
        if (Env.Context.ContainsKey("active_wave_id")) {
            var wave = Env.Ref<StockPickingBatch>("active_wave_id");
            return _AddToWave(wave);
        }
        var view = Env.Ref<View>("stock_picking_batch.stock_add_to_wave_form");
        return new Dictionary<string, object>() {
            { "name", "Add to Wave" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "stock.add.to.wave" },
            { "views", new List<object> { new List<object> { view.Id, "form" } } },
            { "view_id", view.Id },
            { "target", "new" }
        };
    }

    private Dictionary<string, object> _AddToWave(StockPickingBatch wave = null) {
        if (wave == null) {
            wave = Env.Ref<StockPickingBatch>().Create(new Dictionary<string, object>() {
                { "IsWave", true },
                { "PickingTypeId", this.PickingTypeId },
                { "UserId", Env.Context.Get("active_owner_id") }
            });
        }

        var lineByPicking = new Dictionary<StockPicking, List<StockMoveLine>>();
        foreach (var line in this) {
            if (!lineByPicking.ContainsKey(line.PickingId)) {
                lineByPicking.Add(line.PickingId, new List<StockMoveLine>());
            }
            lineByPicking[line.PickingId].Add(line);
        }

        var pickingToWaveValsList = new List<Dictionary<string, object>>();
        foreach (var picking in lineByPicking.Keys) {
            var lineByMove = new Dictionary<StockMove, List<StockMoveLine>>();
            var qtyByMove = new Dictionary<StockMove, double>();

            foreach (var line in lineByPicking[picking]) {
                var move = line.MoveId;
                if (!lineByMove.ContainsKey(move)) {
                    lineByMove.Add(move, new List<StockMoveLine>());
                }
                lineByMove[move].Add(line);

                var qty = line.ProductUomId.ComputeQuantity(line.Quantity, line.ProductId.UomId, "HALF-UP");
                if (!qtyByMove.ContainsKey(move)) {
                    qtyByMove.Add(move, qty);
                }
                else {
                    qtyByMove[move] += qty;
                }
            }

            if (lineByPicking[picking].Count == picking.MoveLineIds.Count && lineByPicking[picking][0].MoveId == picking.MoveIds[0]) {
                var moveComplete = true;
                foreach (var move in qtyByMove.Keys) {
                    if (!Utils.FloatUtils.FloatCompare(move.ProductQty, qtyByMove[move], move.ProductUom.Rounding)) {
                        moveComplete = false;
                        break;
                    }
                }
                if (moveComplete) {
                    wave.PickingIds = new List<Command> { Command.Link(picking.Id) };
                    continue;
                }
            }

            var pickingToWaveVals = picking.CopyData(new Dictionary<string, object>() {
                { "MoveIds", new List<Command>() },
                { "MoveLineIds", new List<Command>() },
                { "BatchId", wave.Id }
            })[0];

            foreach (var move in lineByMove.Keys) {
                pickingToWaveVals["MoveLineIds"] = lineByMove[move].Select(l => Command.Link(l.Id)).ToList();
                if (lineByMove[move].Count == move.MoveLineIds.Count) {
                    pickingToWaveVals["MoveIds"].Add(Command.Link(move.Id));
                    continue;
                }
                var qty = qtyByMove[move];
                var newMove = move.Split(qty);
                newMove[0]["MoveLineIds"] = lineByMove[move].Select(l => Command.Set(l.Id)).ToList();
                pickingToWaveVals["MoveIds"].Add(Command.Create(newMove[0]));
            }

            pickingToWaveValsList.Add(pickingToWaveVals);
        }

        if (pickingToWaveValsList.Count > 0) {
            Env.Ref<StockPicking>().Create(pickingToWaveValsList);
        }
        wave.ActionConfirm();
        return new Dictionary<string, object>();
    }
}
