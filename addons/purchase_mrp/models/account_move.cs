csharp
public partial class AccountMoveLine {
    public virtual IEnumerable<StockValuationLayer> GetStockValuationLayers(AccountMove move) {
        var layers = Env.Call<IEnumerable<StockValuationLayer>>("Account.AccountMoveLine", "_GetStockValuationLayers", move);
        return layers.Where(svl => svl.ProductId == this.ProductId);
    }
}
