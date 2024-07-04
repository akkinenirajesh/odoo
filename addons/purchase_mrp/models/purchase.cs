csharp
public partial class PurchaseOrder
{
    public int MrpProductionCount { get; set; }

    public void ComputeMrpProductionCount()
    {
        this.MrpProductionCount = GetMrpProductions().Count;
    }

    public List<MrpProduction> GetMrpProductions()
    {
        return this.OrderLines.SelectMany(l => l.MoveDestIds.GroupIds.MrpProductionIds).Union(this.OrderLines.MoveIds.MoveDestIds.GroupIds.MrpProductionIds).ToList();
    }

    public dynamic ActionViewMrpProductions()
    {
        var mrpProductionIds = GetMrpProductions().Select(m => m.Id).ToList();

        var action = new
        {
            res_model = "Mrp.Production",
            type = "ir.actions.act_window"
        };

        if (mrpProductionIds.Count == 1)
        {
            action = new
            {
                action.res_model,
                action.type,
                view_mode = "form",
                res_id = mrpProductionIds[0]
            };
        }
        else
        {
            action = new
            {
                action.res_model,
                action.type,
                name = $"Manufacturing Source of {this.Name}",
                domain = new[] { ("id", "in", mrpProductionIds) },
                view_mode = "tree,form"
            };
        }

        return action;
    }
}

public partial class PurchaseOrderLine
{
    public double QtyReceived { get; set; }

    public void ComputeQtyReceived()
    {
        var kitLines = Env.Get<PurchaseOrderLine>().Collection;
        var linesStock = this.Where(l => l.QtyReceivedMethod == "stock_moves" && l.MoveIds != null && l.State != "cancel").ToList();
        var productByCompany = new Dictionary<Company, HashSet<int>>();
        foreach (var line in linesStock)
        {
            if (!productByCompany.ContainsKey(line.Company))
            {
                productByCompany.Add(line.Company, new HashSet<int>());
            }
            productByCompany[line.Company].Add(line.ProductId.Id);
        }
        var kitsByCompany = productByCompany.ToDictionary(
            kvp => kvp.Key,
            kvp => Env.Get<MrpBom>()._bom_find(Env.Get<ProductProduct>().Collection.Where(p => kvp.Value.Contains(p.Id)).ToList(), company_id: kvp.Key.Id, bom_type: "phantom")
        );
        foreach (var line in linesStock)
        {
            var kitBom = kitsByCompany[line.Company].GetValueOrDefault(line.ProductId);
            if (kitBom != null)
            {
                var moves = line.MoveIds.Where(m => m.State == "done" && !m.Scrapped).ToList();
                var orderQty = line.ProductUom.ComputeQuantity(line.ProductUomQty, kitBom.ProductUom);
                var filters = new Dictionary<string, Func<Move, bool>>
                {
                    {"incoming_moves", m => m.LocationId.Usage == "supplier" && (!m.OriginReturnedMoveId || (m.OriginReturnedMoveId && m.ToRefund)) },
                    {"outgoing_moves", m => m.LocationId.Usage != "supplier" && m.ToRefund }
                };
                line.QtyReceived = moves.ComputeKitQuantities(line.ProductId, orderQty, kitBom, filters);
                kitLines.Add(line);
            }
        }
        Env.Get<PurchaseOrderLine>().Collection.Where(l => !kitLines.Contains(l)).ComputeQtyReceived();
    }

    public List<(PurchaseOrder, User)> GetUpstreamDocumentsAndResponsibles(HashSet<PurchaseOrderLine> visited)
    {
        return new List<(PurchaseOrder, User)> { (this.OrderId, this.OrderId.UserId, visited) };
    }

    public double GetQtyProcurement()
    {
        var bom = Env.Get<MrpBom>()._bom_find(this.ProductId, bom_type: "phantom")[this.ProductId];
        if (bom != null && Env.Context.ContainsKey("previous_product_qty"))
        {
            return Env.Context["previous_product_qty"].GetOrDefault<double>(this.Id, 0.0);
        }
        return base.GetQtyProcurement();
    }

    public double GetMoveDestsInitialDemand(List<StockMove> moveDests)
    {
        var kitBom = Env.Get<MrpBom>()._bom_find(this.ProductId, bom_type: "phantom")[this.ProductId];
        if (kitBom != null)
        {
            var filters = new Dictionary<string, Func<Move, bool>>
            {
                {"incoming_moves", m => true },
                {"outgoing_moves", m => false }
            };
            return moveDests.ComputeKitQuantities(this.ProductId, this.ProductQty, kitBom, filters);
        }
        return base.GetMoveDestsInitialDemand(moveDests);
    }
}
