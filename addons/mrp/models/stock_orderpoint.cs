csharp
public partial class Mrp.StockWarehouseOrderpoint {
    public virtual object GetReplenishmentOrderNotification() {
        if (this.Id == 0) return null;
        var domain = new[] { new[] { "OrderpointId", "in", new object[] { this.Id } } };
        if (Env.Context.ContainsKey("written_after")) {
            domain = AND(domain, new[] { new[] { "WriteDate", ">=", Env.Context["written_after"] } });
        }
        var production = Env.Model("mrp.production").Search(domain, limit: 1);
        if (production != null) {
            var action = Env.Ref("mrp.action_mrp_production_form");
            return new {
                type = "ir.actions.client",
                tag = "display_notification",
                params = new {
                    title = _("The following replenishment order has been generated"),
                    message = production.Name,
                    links = new[] {
                        new {
                            label = production.Name,
                            url = $"/web#action={action.Id}&id={production.Id}&model=mrp.production"
                        }
                    },
                    sticky = false
                }
            };
        }
        return base.GetReplenishmentOrderNotification();
    }
    public virtual void ComputeShowBom() {
        var manufactureRoute = Env.Model("stock.rule").SearchRead(new[] { new[] { "Action", "=", "manufacture" } }, new[] { "RouteId" });
        foreach (var orderpoint in this) {
            orderpoint.ShowBom = manufactureRoute.Any(x => x.RouteId[0] == orderpoint.RouteId.Id);
        }
    }
    public virtual object ComputeVisibilityDays() {
        var res = base.ComputeVisibilityDays();
        foreach (var orderpoint in this) {
            if (orderpoint.RuleIds.Any(x => x.Action == "manufacture")) {
                orderpoint.VisibilityDays = orderpoint.ManufacturingVisibilityDays;
            }
        }
        return res;
    }
    public virtual object SetVisibilityDays() {
        var res = base.SetVisibilityDays();
        foreach (var orderpoint in this) {
            if (orderpoint.RuleIds.Any(x => x.Action == "manufacture")) {
                orderpoint.ManufacturingVisibilityDays = orderpoint.VisibilityDays;
            }
        }
        return res;
    }
    public virtual object ComputeDaysToOrder() {
        var res = base.ComputeDaysToOrder();
        foreach (var orderpoint in this) {
            if (orderpoint.RuleIds.Any(x => x.Action == "manufacture")) {
                var boms = orderpoint.ProductId.VariantBomIds != null ? orderpoint.ProductId.VariantBomIds : orderpoint.ProductId.BomIds;
                orderpoint.DaysToOrder = boms.Any() ? boms[0].DaysToPrepareMo : 0;
            }
        }
        return res;
    }
    public virtual object QuantityInProgress() {
        var bomKits = Env.Model("mrp.bom")._BomFind(this.ProductId, bomType: "phantom");
        var bomKitOrderpoints = new Dictionary<Mrp.StockWarehouseOrderpoint, Mrp.Bom>();
        foreach (var orderpoint in this) {
            if (bomKits.ContainsKey(orderpoint.ProductId)) {
                bomKitOrderpoints[orderpoint] = bomKits[orderpoint.ProductId];
            }
        }
        var orderpointsWithoutKit = this - new Mrp.StockWarehouseOrderpoint[] { };
        var res = base.QuantityInProgress(orderpointsWithoutKit);
        foreach (var orderpoint in bomKitOrderpoints) {
            var dummy, bomSubLines = orderpoint.Value.Explode(orderpoint.Key.ProductId, 1);
            var ratiosQtyAvailable = new List<double>();
            var ratiosTotal = new List<double>();
            foreach (var bomLine in bomSubLines) {
                var component = bomLine.Item1.ProductId;
                if (!component.IsStorable || float_is_zero(bomLine.Item2.Qty, bomLine.Item1.ProductUomId.Rounding)) continue;
                var uomQtyPerKit = bomLine.Item2.Qty / bomLine.Item2.OriginalQty;
                var qtyPerKit = bomLine.Item1.ProductUomId.ComputeQuantity(uomQtyPerKit, component.UomId);
                if (qtyPerKit == 0) continue;
                var qtyByProductLocation = component._GetQuantityInProgress(new[] { orderpoint.Key.LocationId.Id });
                var qtyInProgress = qtyByProductLocation.GetValueOrDefault((component.Id, orderpoint.Key.LocationId.Id), 0.0);
                var qtyAvailable = component.QtyAvailable / qtyPerKit;
                ratiosQtyAvailable.Add(qtyAvailable);
                ratiosTotal.Add(qtyAvailable + (qtyInProgress / qtyPerKit));
            }
            var productQty = Math.Min(ratiosTotal.Any() ? ratiosTotal.Min() : 0, ratiosQtyAvailable.Any() ? ratiosQtyAvailable.Min() : 0);
            res[orderpoint.Key.Id] = orderpoint.Key.ProductId.UomId.ComputeQuantity(productQty, orderpoint.Key.ProductUom, round: false);
        }
        var bomManufacture = Env.Model("mrp.bom")._BomFind(orderpointsWithoutKit.ProductId, bomType: "normal");
        bomManufacture = new Mrp.Bom[] { };
        var productionsGroup = Env.Model("mrp.production")._ReadGroup(
            new[] {
                new[] { "BomId", "in", bomManufacture.Select(x => x.Id).ToArray() },
                new[] { "State", "=", "draft" },
                new[] { "OrderpointId", "in", orderpointsWithoutKit.Select(x => x.Id).ToArray() },
                new[] { "Id", "not in", Env.Context.GetValueOrDefault("ignore_mo_ids", new object[] { }) }
            },
            new[] { "OrderpointId", "ProductUomId" },
            new[] { "product_qty:sum" });
        foreach (var orderpoint in productionsGroup) {
            res[orderpoint.OrderpointId[0]] += orderpoint.ProductUomId.ComputeQuantity(orderpoint.product_qty, orderpoint.OrderpointId.ProductUom, round: false);
        }
        return res;
    }
    public virtual object GetQtyMultipleToOrder() {
        if (this.Id == 0) return null;
        var qtyMultipleToOrder = base.GetQtyMultipleToOrder();
        if (this.RuleIds.Any(x => x.Action == "manufacture")) {
            var bom = Env.Model("mrp.bom")._BomFind(this.ProductId, bomType: "normal")[this.ProductId];
            return bom.ProductUomId.ComputeQuantity(bom.ProductQty, this.ProductUom);
        }
        return qtyMultipleToOrder;
    }
    public virtual void SetDefaultRouteId() {
        var routeIds = Env.Model("stock.rule").Search(new[] { new[] { "Action", "=", "manufacture" } }).RouteId;
        foreach (var orderpoint in this) {
            if (orderpoint.ProductId.BomIds.Any() == false) continue;
            var routeId = orderpoint.RuleIds.RouteId.Intersect(routeIds).ToArray();
            if (routeId.Length == 0) continue;
            orderpoint.RouteId = routeId[0].Id;
        }
        base.SetDefaultRouteId();
    }
    public virtual object PrepareProcurementValues(DateTime? date = null, bool? group = null) {
        var values = base.PrepareProcurementValues(date, group);
        values["BomId"] = this.BomId;
        return values;
    }
    public virtual void PostProcessScheduler() {
        Env.Model("mrp.production").Search(new[] {
            new[] { "OrderpointId", "in", this.Select(x => x.Id).ToArray() },
            new[] { "MoveRawIds", "!=", false },
            new[] { "State", "=", "draft" }
        }).ActionConfirm();
        base.PostProcessScheduler();
    }
}
