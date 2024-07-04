C#
public partial class PurchaseMrp.MrpProduction
{
    public int PurchaseOrderCount { get; set; }

    public void ComputePurchaseOrderCount()
    {
        this.PurchaseOrderCount = Env.Model("Purchase.Order").Search(new[] {
            Env.Model("Stock.Move").Search(new[] {
                Env.Model("Stock.Move").Search(new[] {
                    this.ProcuremenGroup.StockMoveIds.CreatedPurchaseLineIds.OrderId.Id
                }, "move_orig_ids").PurchaseLineId.OrderId.Id
            }, "created_purchase_line_ids").OrderId.Id
        }).Count;
    }

    public void ActionViewPurchaseOrders()
    {
        var purchaseOrderIds = new List<int>() {
            Env.Model("Stock.Move").Search(new[] {
                this.ProcuremenGroup.StockMoveIds.CreatedPurchaseLineIds.OrderId.Id
            }, "created_purchase_line_ids").OrderId.Id,
            Env.Model("Stock.Move").Search(new[] {
                this.ProcuremenGroup.StockMoveIds.MoveOrigIds.PurchaseLineId.OrderId.Id
            }, "move_orig_ids").PurchaseLineId.OrderId.Id
        };

        var action = new {
            ResModel = "Purchase.Order",
            Type = "ir.actions.act_window",
        };

        if (purchaseOrderIds.Count == 1) {
            action.Update(new {
                ViewMode = "form",
                ResId = purchaseOrderIds[0],
            });
        } else {
            action.Update(new {
                Name = $"Purchase Order generated from {this.Name}",
                Domain = new[] { ("Id", "in", purchaseOrderIds) },
                ViewMode = "tree,form",
            });
        }

        Env.Action(action);
    }

    public string GetDocumentIterateKey(Stock.Move moveRawId)
    {
        var iterateKey = base.GetDocumentIterateKey(moveRawId);

        if (!iterateKey && moveRawId.CreatedPurchaseLineIds != null) {
            iterateKey = "created_purchase_line_ids";
        }

        return iterateKey;
    }

    public Dictionary<int, Dictionary<string, object>> PrepareMergeOrigLinks()
    {
        var origs = base.PrepareMergeOrigLinks();

        foreach (var move in this.MoveRawIds) {
            if (move.MoveOrigIds == null || move.CreatedPurchaseLineIds == null) {
                continue;
            }

            origs[move.BomLineId.Id].TryAdd("created_purchase_line_ids", new List<int>());
            origs[move.BomLineId.Id]["created_purchase_line_ids"].AddRange(move.CreatedPurchaseLineIds.Id);
        }

        foreach (var vals in origs) {
            if (vals.Value.ContainsKey("created_purchase_line_ids")) {
                vals.Value["created_purchase_line_ids"] = new Command(vals.Value["created_purchase_line_ids"]);
            } else {
                vals.Value["created_purchase_line_ids"] = new List<int>();
            }
        }

        return origs;
    }
}
