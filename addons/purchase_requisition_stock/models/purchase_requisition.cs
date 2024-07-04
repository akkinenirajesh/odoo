csharp
public partial class PurchaseRequisition
{
    public virtual stock.warehouse Warehouse { get; set; }
    public virtual stock.picking.type PickingType { get; set; }

    public stock.picking.type DefaultPickingTypeId()
    {
        return Env.Search<stock.picking.type>(new object[] { ("warehouse_id.company_id", "=", Env.Company.Id), ("code", "=", "incoming") }, 1);
    }
}

public partial class PurchaseRequisitionLine
{
    public virtual stock.move MoveDest { get; set; }

    public Dictionary<string, object> PreparePurchaseOrderLine(string name, double productQty = 0.0, double priceUnit = 0.0, List<stock.tax> taxesIds = null)
    {
        Dictionary<string, object> res = base.PreparePurchaseOrderLine(name, productQty, priceUnit, taxesIds);
        res["move_dest_ids"] = MoveDest != null ? new List<object> { new Dictionary<string, object> { { "id", MoveDest.Id } } } : new List<object>();
        return res;
    }
}
