C#
public partial class StockMoveLine {

    public StockMoveLine Create(List<Dictionary<string, object>> valsList) {
        var analyticMoveToRecompute = new HashSet<int>();
        var moveLines = Env.Call("StockMoveLine", "Create", valsList) as List<StockMoveLine>;
        foreach (var moveLine in moveLines) {
            var move = moveLine.MoveId;
            analyticMoveToRecompute.Add(move.Id);
            if (moveLine.State != "done") {
                continue;
            }
            var rounding = move.ProductId.UomId.Rounding;
            var diff = move.ProductUom.ComputeQuantity(moveLine.Quantity, move.ProductId.UomId);
            if (diff.IsZero(rounding)) {
                continue;
            }
            CreateCorrectionSvl(move, diff);
        }
        if (analyticMoveToRecompute.Count > 0) {
            Env.Call("StockMove", "_AccountAnalyticEntryMove", analyticMoveToRecompute.ToList());
        }
        return moveLines.FirstOrDefault();
    }

    public void Write(Dictionary<string, object> vals) {
        var analyticMoveToRecompute = new HashSet<int>();
        if (vals.ContainsKey("Quantity") || vals.ContainsKey("MoveId")) {
            foreach (var moveLine in this) {
                var moveId = vals.ContainsKey("MoveId") ? (int)vals["MoveId"] : moveLine.MoveId.Id;
                analyticMoveToRecompute.Add(moveId);
            }
        }
        if (vals.ContainsKey("Quantity")) {
            foreach (var moveLine in this) {
                if (moveLine.State != "done") {
                    continue;
                }
                var move = moveLine.MoveId;
                if (vals["Quantity"].Equals(moveLine.Quantity)) {
                    continue;
                }
                var rounding = move.ProductId.UomId.Rounding;
                var diff = move.ProductUom.ComputeQuantity((decimal)vals["Quantity"] - moveLine.Quantity, move.ProductId.UomId, "HALF-UP");
                if (diff.IsZero(rounding)) {
                    continue;
                }
                CreateCorrectionSvl(move, diff);
            }
        }
        Env.Call("StockMoveLine", "Write", new List<StockMoveLine> { this }, vals);
        if (analyticMoveToRecompute.Count > 0) {
            Env.Call("StockMove", "_AccountAnalyticEntryMove", analyticMoveToRecompute.ToList());
        }
    }

    public void Unlink() {
        Env.Call("StockMoveLine", "Unlink", new List<StockMoveLine> { this });
        MoveId._AccountAnalyticEntryMove();
    }

    private void CreateCorrectionSvl(StockMove move, decimal diff) {
        var stockValuationLayers = Env.Call("StockValuationLayer", "Create");
        if (move._IsIn() && diff > 0 || move._IsOut() && diff < 0) {
            move.ProductPriceUpdateBeforeDone(diff);
            stockValuationLayers |= move._CreateInSvl(Math.Abs(diff));
            if (move.ProductId.CostMethod == "average" || move.ProductId.CostMethod == "fifo") {
                move.ProductId._RunFifoVacuum(move.CompanyID);
            }
        } else if (move._IsIn() && diff < 0 || move._IsOut() && diff > 0) {
            stockValuationLayers |= move._CreateOutSvl(Math.Abs(diff));
        } else if (move._IsDropshipped() && diff > 0 || move._IsDropshippedReturned() && diff < 0) {
            stockValuationLayers |= move._CreateDropshippedSvl(Math.Abs(diff));
        } else if (move._IsDropshipped() && diff < 0 || move._IsDropshippedReturned() && diff > 0) {
            stockValuationLayers |= move._CreateDropshippedReturnedSvl(Math.Abs(diff));
        }

        stockValuationLayers._ValidateAccountingEntries();
    }

    private bool ShouldExcludeForValuation() {
        return OwnerId != null && OwnerId != CompanyID.PartnerId;
    }
}
