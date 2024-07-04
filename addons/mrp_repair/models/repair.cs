csharp
public partial class MrpRepair.Repair 
{
    public void Create(List<Dictionary<string, object>> valsList)
    {
        var orders = Env.Call("Create", valsList, "MrpRepair.Repair");
        orders.Call("ActionExplode");
    }

    public void Write(Dictionary<string, object> vals)
    {
        var res = Env.Call("Write", vals, "MrpRepair.Repair");
        this.Call("ActionExplode");
    }

    public void ActionExplode()
    {
        var linesToUnlinkIds = new HashSet<long>();
        var lineValsList = new List<Dictionary<string, object>>();
        foreach (var op in this.Get("MoveIds"))
        {
            var bom = Env.Call("BomFind", op.Get("ProductId"), new Dictionary<string, object> { { "CompanyId", op.Get("CompanyId") }, { "BomType", "phantom" } }, "Mrp.Bom");
            if (bom == null)
                continue;
            var factor = Env.Call("_computeQuantity", op.Get("ProductUom"), op.Get("ProductUomQty"), bom.Get("ProductUomId"), "Product.Uom");
            factor = factor / bom.Get("ProductQty");
            var _boms = Env.Call("Explode", bom, op.Get("ProductId"), factor, new Dictionary<string, object> { { "PickingTypeId", bom.Get("PickingTypeId") } }, "Mrp.Bom");
            var lines = _boms.GetValue("lines");
            foreach (var bomLine in lines)
            {
                if (bomLine.Get("ProductId").Call("Type", "Product.Product") != "service")
                {
                    lineValsList.Add(op.Call("_PreparePhantomLineVals", bomLine, bomLine.GetValue("qty")));
                }
            }
            linesToUnlinkIds.Add(op.Get("Id"));
        }
        Env.Call("Unlink", linesToUnlinkIds, "Stock.Move");
        if (lineValsList.Count > 0)
        {
            Env.Call("Create", lineValsList, "Stock.Move");
        }
    }

    public Dictionary<string, object> GetActionAddFromCatalogExtraContext()
    {
        var bom = Env.Call("BomFind", this.Get("ProductId"), new Dictionary<string, object> { { "CompanyId", this.Get("CompanyId") } }, "Mrp.Bom");
        var productIds = new List<long>();
        if (bom != null)
        {
            foreach (var line in bom.Get("BomLineIds"))
            {
                productIds.Add(line.Get("ProductId"));
            }
        }
        var extraContext = Env.Call("_GetActionAddFromCatalogExtraContext", "Repair.Repair");
        extraContext["CatalogBomProductIds"] = productIds;
        extraContext["SearchDefaultBomParts"] = productIds.Count > 0;
        return extraContext;
    }
}
