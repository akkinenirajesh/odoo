csharp
public partial class StockMove {
    public virtual void SearchPickingForAssignationDomain() {
        var domain = Env.Call("Stock.StockMove", "_search_picking_for_assignation_domain");
        domain = Env.Call("expression", "AND", new object[] { domain, new object[] { "OR", new object[] { "BatchId", "=", false }, new object[] { "BatchId.IsWave", "=", false } } });
        return domain;
    }

    public virtual void ActionCancel() {
        var res = Env.Call("Stock.StockMove", "_action_cancel");
        foreach (var picking in this.PickingId) {
            if (picking.State == "cancel" && picking.BatchId != null && picking.BatchId.PickingIds.Any(p => p.State != "cancel")) {
                picking.BatchId = null;
            }
        }
        return res;
    }

    public virtual void AssignPickingPostProcess(bool new = false) {
        Env.Call("Stock.StockMove", "_assign_picking_post_process", new object[] { new });
        foreach (var picking in this.PickingId) {
            picking.FindAutoBatch();
        }
    }

    public virtual void Write(object vals) {
        var res = Env.Call("Stock.StockMove", "write", new object[] { this, vals });
        if (vals.ContainsKey("State") && vals["State"].ToString() == "assigned") {
            foreach (var picking in this.PickingId) {
                if (picking.State != "assigned") {
                    continue;
                }
                picking.FindAutoBatch();
            }
        }
        return res;
    }
}
