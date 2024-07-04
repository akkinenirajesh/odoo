csharp
public partial class MrpSubcontractingPurchase.StockPicking 
{
    public int SubcontractingSourcePurchaseCount { get; set; }

    public void ComputeSubcontractingSourcePurchaseCount()
    {
        foreach (var picking in this)
        {
            picking.SubcontractingSourcePurchaseCount = GetSubcontractingSourcePurchase().Count();
        }
    }

    public System.Collections.Generic.List<Purchase.PurchaseOrder> GetSubcontractingSourcePurchase()
    {
        var movesSubcontracted = this.MoveIds.MoveDestIds.RawMaterialProductionId.MoveFinishedIds.MoveDestIds.Where(m => m.IsSubcontract);
        return movesSubcontracted.PurchaseLineId.OrderId;
    }

    public System.Collections.Generic.Dictionary<string, object> ActionViewSubcontractingSourcePurchase()
    {
        var purchaseOrderIds = this.GetSubcontractingSourcePurchase().Select(x => x.Id).ToList();
        var action = new System.Collections.Generic.Dictionary<string, object>();
        action.Add("res_model", "Purchase.PurchaseOrder");
        action.Add("type", "ir.actions.act_window");
        if (purchaseOrderIds.Count == 1)
        {
            action.Add("view_mode", "form");
            action.Add("res_id", purchaseOrderIds[0]);
        }
        else
        {
            action.Add("name", string.Format(_("Source PO of {0}", this.Name)));
            action.Add("domain", new[] { new[] { "id", "in", purchaseOrderIds } });
            action.Add("view_mode", "tree,form");
        }
        return action;
    }
}
