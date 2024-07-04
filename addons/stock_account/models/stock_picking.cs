C#
public partial class StockPicking {

    public StockPickingActionViewStockValuationLayers( ) {
        var scraps = Env.Get<StockScrap>().Search(new[] { ("Picking", this.Id) });
        var domain = new[] { ("Id", "in", (this.MoveIds + scraps.MoveIds).StockValuationLayerIds.Ids) };
        var action = Env.Get<IrActionsActions>()._ForXmlId("stock_account.stock_valuation_layer_action");
        var context = action.Context;
        context.Update(Env.Context);
        context.Add("NoAtDate", true);
        return new Dictionary<string, object> {
            { "action", action },
            { "domain", domain },
            { "context", context },
        };
    }

}
