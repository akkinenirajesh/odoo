C#
public partial class SaleProjectStock.SaleOrderLine {

    public Dictionary<int, Tuple<int, int>> GetActionPerItem() {
        Dictionary<int, Tuple<int, int>> actionPerSol = Env.Ref<SaleProjectStock.SaleOrderLine>("SaleProjectStock.SaleOrderLine")._GetActionPerItem();
        int stockMoveAction = Env.Ref<object>("sale_project_stock.stock_move_per_sale_order_line_action").Id;
        Dictionary<int, List<int>> stockMoveIdsPerSol = new Dictionary<int, List<int>>();
        if (Env.User.HasGroup("stock.group_stock_user")) {
            List<object> stockMoveReadGroup = Env.Model<Stock.Move>()._ReadGroup(new List<object>() { new object[] { "SaleLineId", "in", this.Id } }, new List<object>() { "SaleLineId" }, new List<object>() { "Id:array_agg" });
            stockMoveIdsPerSol = stockMoveReadGroup.ToDictionary(x => (int)x.GetValue(0), x => (List<int>)x.GetValue(1));
        }
        foreach (SaleProjectStock.SaleOrderLine sol in this.Collection()) {
            List<int> stockMoveIds = stockMoveIdsPerSol.GetValueOrDefault(sol.Id, new List<int>());
            if (!sol.IsService && stockMoveIds.Count > 0) {
                actionPerSol[sol.Id] = new Tuple<int, int>(stockMoveAction, stockMoveIds.Count == 1 ? stockMoveIds[0] : 0);
            }
        }
        return actionPerSol;
    }

}
