csharp
public partial class StockMove 
{
    public bool IsPurchaseReturn { get; set; }

    public void ComputeIsPurchaseReturn()
    {
        var res = Env.Call("stock.move", "_is_purchase_return");
        IsPurchaseReturn = res || Env.Call("stock.move", "_is_subcontract_return");
    }
}
