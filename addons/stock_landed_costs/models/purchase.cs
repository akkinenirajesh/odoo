csharp
public partial class PurchaseOrderLine {
    public virtual bool IsLandedCostsLine { get; set; }

    public virtual void _PrepareAccountMoveLine(object move) {
        var res = Env.Call("Purchase.PurchaseOrderLine", "_PrepareAccountMoveLine", move);
        res.Update(new Dictionary<string, object>() { { "IsLandedCostsLine", this.Product.LandedCostOk } });
        return res;
    }
}
