csharp
public partial class Repair.StockLot {
    public void ComputeRepairLineIds() {
        var repairOrders = new Dictionary<int, Repair.RepairOrder>();

        var repairMoves = Env.Search<Stock.Move>(m => m.RepairId != null && m.RepairLineType != null && m.MoveLineIds.Any(ml => ml.LotId.IsIn(this.Id)) && m.State == "done");

        foreach (var repairLine in repairMoves) {
            foreach (var lotId in repairLine.LotIds.Select(l => l.Id)) {
                if (!repairOrders.ContainsKey(lotId)) {
                    repairOrders.Add(lotId, Env.New<Repair.RepairOrder>());
                }
                repairOrders[lotId] |= repairLine.RepairId;
            }
        }
        this.RepairLineIds = repairOrders[this.Id];
        this.RepairPartCount = this.RepairLineIds.Count();
    }

    public void ComputeInRepairCount() {
        var lotData = Env.Search<Repair.RepairOrder>(r => r.LotId.IsIn(this.Id) && r.State != "done" && r.State != "cancel").ReadGroup(new string[] { "LotId" }, new string[] { "__count" }, null);
        var result = lotData.ToDictionary(ld => ld.LotId, ld => ld.__count);
        this.InRepairCount = result.GetValueOrDefault(this.Id, 0);
    }

    public void ComputeRepairedCount() {
        var lotData = Env.Search<Repair.RepairOrder>(r => r.LotId.IsIn(this.Id) && r.State == "done").ReadGroup(new string[] { "LotId" }, new string[] { "__count" }, null);
        var result = lotData.ToDictionary(ld => ld.LotId, ld => ld.__count);
        this.RepairedCount = result.GetValueOrDefault(this.Id, 0);
    }

    public Ir.Action ActionLotOpenRepairs() {
        var action = Env.Get<Ir.Actions.Actions>().ForXmlId("repair.action_repair_order_tree");

        action.Domain = new[] { new Ir.DomainCondition("lot_id", "=", this.Id) };

        action.Context = new Ir.Context {
            { "default_product_id", this.ProductId.Id },
            { "default_lot_id", this.Id },
            { "default_company_id", this.CompanyId.Id },
        };
        return action;
    }

    public Ir.Action ActionViewRo() {
        var action = new Ir.Action {
            ResModel = "repair.order",
            Type = "ir.actions.act_window"
        };

        if (this.RepairLineIds.Count() == 1) {
            action.ViewMode = "form";
            action.ResId = this.RepairLineIds[0].Id;
        } else {
            action.Name = $"Repair orders of {this.Name}";
            action.Domain = new[] { new Ir.DomainCondition("id", "in", this.RepairLineIds.Select(rl => rl.Id).ToArray()) };
            action.ViewMode = "tree,form";
        }
        return action;
    }
}
