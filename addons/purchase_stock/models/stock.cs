csharp
public partial class PurchaseStock.StockPicking {
    public Purchase.PurchaseOrder PurchaseId { get; set; }
}

public partial class PurchaseStock.StockWarehouse {
    public bool BuyToResupply { get; set; }
    public Stock.StockRule BuyPullId { get; set; }

    public void _GenerateGlobalRouteRulesValues() {
        var rules = base._GenerateGlobalRouteRulesValues();
        rules.Update(new Dictionary<string, object>() {
            { "BuyPullId", new Dictionary<string, object>() {
                { "Depends", new string[] { "ReceptionSteps", "BuyToResupply" } },
                { "CreateValues", new Dictionary<string, object>() {
                    { "Action", "buy" },
                    { "PickingTypeId", this.InTypeId.Id },
                    { "GroupPropagationOption", "none" },
                    { "CompanyId", this.CompanyId.Id },
                    { "RouteId", Env.Get("PurchaseStock.RouteWarehouse0Buy").SearchRead(new object[] { "name", "=", "_('Buy')" }).FirstOrDefault()["id"] },
                    { "PropagateCancel", this.ReceptionSteps != "one_step" }
                } },
                { "UpdateValues", new Dictionary<string, object>() {
                    { "Active", this.BuyToResupply },
                    { "Name", "_" + this.LotStockId.Name + " - Buy" },
                    { "LocationDestId", this.LotStockId.Id },
                    { "PropagateCancel", this.ReceptionSteps != "one_step" }
                } }
            }
        });
        return rules;
    }

    public List<Stock.StockRule> _GetAllRoutes() {
        var routes = base._GetAllRoutes();
        routes.AddRange(this.Where(w => w.BuyToResupply && w.BuyPullId != null && w.BuyPullId.RouteId != null).SelectMany(s => s.BuyPullId.RouteId).ToList());
        return routes;
    }

    public Dictionary<int, object> GetRulesDict() {
        var result = base.GetRulesDict();
        foreach (var warehouse in this) {
            result[warehouse.Id].Update(warehouse._GetReceiveRulesDict());
        }
        return result;
    }

    public Dictionary<string, object> _GetRoutesValues() {
        var routes = base._GetRoutesValues();
        routes.Update(this._GetReceiveRoutesValues("BuyToResupply"));
        return routes;
    }

    public void _UpdateNameAndCode(string name = null, string code = null) {
        base._UpdateNameAndCode(name, code);
        if (this.BuyPullId != null && name != null) {
            this.BuyPullId.Write(new Dictionary<string, object>() { { "Name", this.BuyPullId.Name.Replace(this.Name, name, 1) } });
        }
    }
}

public partial class PurchaseStock.ReturnPicking {
    public Res.Partner PartnerId { get; set; }

    public Dictionary<string, object> _PrepareMoveDefaultValues(Stock.ReturnPickingLine returnLine, Stock.StockPicking newPicking) {
        var vals = base._PrepareMoveDefaultValues(returnLine, newPicking);
        if (this.LocationId.Usage == "supplier") {
            var result = returnLine.MoveId._GetPurchaseLineAndPartnerFromChain();
            vals["PurchaseLineId"] = result.Item1;
            vals["PartnerId"] = result.Item2;
        }
        return vals;
    }

    public Stock.StockPicking _CreateReturn() {
        var picking = base._CreateReturn();
        if (picking.MoveIds.Where(w => w.PartnerId != null).Count() == 1) {
            picking.PartnerId = picking.MoveIds.Where(w => w.PartnerId != null).FirstOrDefault().PartnerId;
        }
        return picking;
    }
}

public partial class PurchaseStock.Orderpoint {
    public bool ShowSupplier { get; set; }
    public Product.Supplierinfo SupplierId { get; set; }
    public Res.Partner VendorId { get; set; }
    public float PurchaseVisibilityDays { get; set; }
    public Res.Partner ProductSupplierId { get; set; }
    public float DaysToOrder { get; set; }

    public void ComputeShowSupplier() {
        var buyRoutes = Env.Get("Stock.StockRule").SearchRead(new object[] { "Action", "=", "buy" }).Select(s => s["RouteId"]).ToList();
        this.ShowSupplier = buyRoutes.Contains(this.RouteId.Id);
    }

    public void ComputeQty() {
        // Extend to add more depends values
        base.ComputeQty();
    }

    public void ComputeLeadDays() {
        base.ComputeLeadDays();
    }

    public void ComputeVisibilityDays() {
        base.ComputeVisibilityDays();
        if (this.RuleIds.Select(s => s.Action).Contains("buy")) {
            this.VisibilityDays = this.PurchaseVisibilityDays;
        }
    }

    public void ComputeProductSupplierId() {
        this.ProductSupplierId = this.ProductTmplId.SellerIds.OrderBy(o => o.Sequence).FirstOrDefault()?.PartnerId;
    }

    public void _SetVisibilityDays() {
        base._SetVisibilityDays();
        if (this.RuleIds.Select(s => s.Action).Contains("buy")) {
            this.PurchaseVisibilityDays = this.VisibilityDays;
        }
    }

    public void ComputeDaysToOrder() {
        base.ComputeDaysToOrder();
        if (this.RuleIds.Select(s => s.Action).Contains("buy")) {
            this.DaysToOrder = this.CompanyId.DaysToPurchase;
        }
    }

    public void ActionViewPurchase() {
        var result = Env.Get("Ir.Actions.ActWindow")._ForXmlId("purchase.purchase_rfq");
        // Remvove the context since the action basically display RFQ and not PO.
        result["context"] = new Dictionary<string, object>();
        var orderLineIds = Env.Get("Purchase.PurchaseOrderLine").Search(new object[] { "OrderpointId", "=", this.Id });
        var purchaseIds = orderLineIds.Select(s => s.OrderId).ToList();
        result["domain"] = "[('id', 'in', %s)]" % purchaseIds;
        return result;
    }

    public Dictionary<string, object> _GetLeadDaysValues() {
        var values = base._GetLeadDaysValues();
        if (this.SupplierId != null) {
            values["Supplierinfo"] = this.SupplierId;
        }
        return values;
    }

    public Dictionary<string, object> _GetReplenishmentOrderNotification() {
        var domain = new object[] { "OrderpointId", "in", this.Id };
        if (Env.Context.ContainsKey("written_after")) {
            domain = new object[] { domain, new object[] { "WriteDate", ">=", Env.Context["written_after"] } };
        }
        var order = Env.Get("Purchase.PurchaseOrderLine").Search(domain, 1).FirstOrDefault()?.OrderId;
        if (order != null) {
            var action = Env.Get("Purchase.ActionRfqForm");
            return new Dictionary<string, object>() {
                { "type", "ir.actions.client" },
                { "tag", "display_notification" },
                { "params", new Dictionary<string, object>() {
                    { "title", _("The following replenishment order has been generated") },
                    { "message", order.DisplayName },
                    { "links", new List<object>() {
                        new Dictionary<string, object>() {
                            { "label", order.DisplayName },
                            { "url", $"/web#action={action.Id}&id={order.Id}&model=purchase.order" }
                        }
                    } },
                    { "sticky", false }
                } }
            };
        }
        return base._GetReplenishmentOrderNotification();
    }

    public Dictionary<string, object> _PrepareProcurementValues(DateTime? date = null, bool group = false) {
        var values = base._PrepareProcurementValues(date, group);
        values["SupplierinfoId"] = this.SupplierId;
        return values;
    }

    public decimal _QuantityInProgress() {
        var res = base._QuantityInProgress();
        var qtyByProductLocation = this.ProductId._GetQuantityInProgress(this.LocationId.Id);
        res += this.ProductId.UomId._ComputeQuantity(qtyByProductLocation.GetValueOrDefault(), this.ProductUom, false);
        return res;
    }

    public void _SetDefaultRouteId() {
        var routeIds = Env.Get("Stock.StockRule").Search(new object[] { "Action", "=", "buy" }).Select(s => s.RouteId).ToList();
        foreach (var orderpoint in this) {
            var routeId = orderpoint.RuleIds.Intersect(routeIds);
            if (orderpoint.ProductId.SellerIds.Count() == 0) {
                continue;
            }
            if (routeId.Count() == 0) {
                continue;
            }
            orderpoint.RouteId = routeId.FirstOrDefault().Id;
        }
        base._SetDefaultRouteId();
    }
}

public partial class PurchaseStock.StockLot {
    public ICollection<Purchase.PurchaseOrder> PurchaseOrderIds { get; set; }
    public int PurchaseOrderCount { get; set; }

    public void ComputePurchaseOrderIds() {
        var purchaseOrders = new Dictionary<int, List<Purchase.PurchaseOrder>>();
        foreach (var moveLine in Env.Get("Stock.StockMoveLine").Search(new object[] { "LotId", "in", this.Id }, new object[] { "State", "=", "done" })) {
            var move = moveLine.MoveId;
            if (move.PickingId.LocationId.Usage.Contains("supplier") && move.PurchaseLineId.OrderId != null) {
                if (!purchaseOrders.ContainsKey(moveLine.LotId.Id)) {
                    purchaseOrders.Add(moveLine.LotId.Id, new List<Purchase.PurchaseOrder>());
                }
                purchaseOrders[moveLine.LotId.Id].Add(move.PurchaseLineId.OrderId);
            }
        }
        foreach (var lot in this) {
            lot.PurchaseOrderIds = purchaseOrders.ContainsKey(lot.Id) ? purchaseOrders[lot.Id] : new List<Purchase.PurchaseOrder>();
            lot.PurchaseOrderCount = lot.PurchaseOrderIds.Count();
        }
    }

    public void ActionViewPo() {
        var action = Env.Get("Ir.Actions.Actions")._ForXmlId("purchase.purchase_form_action");
        action["domain"] = new object[] { "id", "in", this.PurchaseOrderIds.Select(s => s.Id).ToList() };
        action["context"] = new Dictionary<string, object>() { { "create", false } };
        return action;
    }
}

public partial class PurchaseStock.ProcurementGroup {
    public static void Run(List<Procurement.Procurement> procurements, bool raiseUserError = true) {
        var whByComp = new Dictionary<Res.Company, List<Stock.StockWarehouse>>();
        foreach (var procurement in procurements) {
            var routes = procurement.Values.GetValueOrDefault("route_ids") as Stock.StockRule;
            if (routes != null && routes.RuleIds.Any(a => a.Action == "buy")) {
                var company = procurement.CompanyId;
                if (!whByComp.ContainsKey(company)) {
                    whByComp.Add(company, Env.Get("Stock.StockWarehouse").Search(new object[] { "CompanyId", "=", company.Id }).ToList());
                }
                var wh = whByComp[company];
                procurement.Values["route_ids"] = wh.ReceptionRouteId;
            }
        }
        base.Run(procurements, raiseUserError);
    }
}
