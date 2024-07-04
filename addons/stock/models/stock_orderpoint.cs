csharp
public partial class StockWarehouseOrderpoint {

    public void ComputeAllowedLocationIds() {
        var locDomain = new[] { "Usage", "in", new[] { "internal", "view" } };
        // We want to keep only the locations
        //  - strictly belonging to our warehouse
        //  - not belonging to any warehouses
        var otherWarehouses = env.StockWarehouse.Search(new[] { "Id", "!=", this.Warehouse.Id });
        foreach (var viewLocationId in otherWarehouses.Select(w => w.ViewLocationId)) {
            locDomain = new[] { "!", "Id", "child_of", viewLocationId.Id };
            locDomain = new[] { "|", "CompanyId", "=", false, "CompanyId", "=", this.CompanyId.Id };
        }
        this.AllowedLocationIds = env.StockLocation.Search(locDomain);
    }

    public void ComputeLeadDays() {
        if (this.Product == null || this.Location == null) {
            this.LeadDaysDate = null;
            return;
        }
        var values = GetLeadDaysValues();
        var leadDays = this.RuleIds.GetLeadDays(this.Product, values);
        this.LeadDaysDate = DateTime.Now.Date.AddDays(leadDays.TotalDelay);
    }

    public void ComputeRules() {
        if (this.Product == null || this.Location == null) {
            this.RuleIds = null;
            return;
        }
        this.RuleIds = this.Product.GetRulesFromLocation(this.Location, this.Route);
    }

    public void ComputeVisibilityDays() {
        this.VisibilityDays = 0;
    }

    public void SetVisibilityDays() {
        // No implementation required
    }

    public void ComputeDaysToOrder() {
        this.DaysToOrder = 0;
    }

    public void CheckProductUom() {
        if (this.Product.UomId.CategoryId != this.ProductUom.CategoryId) {
            throw new Exception("You have to select a product unit of measure that is in the same category as the default unit of measure of the product");
        }
    }

    public void ComputeWarehouse() {
        if (this.Location.WarehouseId != null) {
            this.Warehouse = this.Location.WarehouseId;
        } else if (this.CompanyId != null) {
            this.Warehouse = env.StockWarehouse.Search(new[] { "CompanyId", "=", this.CompanyId.Id }, limit: 1);
        }
    }

    public void ComputeLocation() {
        var warehouse = this.Warehouse;
        if (warehouse == null) {
            warehouse = env.StockWarehouse.Search(new[] { "CompanyId", "=", this.CompanyId.Id }, limit: 1);
        }
        this.Location = warehouse.LotStockId;
    }

    public void ComputeUnwantedReplenish() {
        if (this.Product == null || this.QtyToOrder == 0 || this.ProductMaxQty < 0) {
            this.UnwantedReplenish = false;
        } else {
            var afterReplenishQty = this.Product.WithContext(company_id: this.CompanyId.Id, location: this.Location.Id).VirtualAvailable + this.QtyToOrder;
            this.UnwantedReplenish = afterReplenishQty > this.ProductMaxQty;
        }
    }

    public void OnChangeProductId() {
        if (this.Product != null) {
            this.ProductUom = this.Product.UomId;
        }
    }

    public void OnChangeRouteId() {
        if (this.Route != null) {
            this.QtyMultiple = GetQtyMultipleToOrder();
        }
    }

    public void Write(params object[] vals) {
        if (vals.Contains("CompanyId")) {
            if (this.CompanyId.Id != (int)vals[1]) {
                throw new Exception("Changing the company of this record is forbidden at this point, you should rather archive it and create a new one.");
            }
        }
        env.Base.Write(this, vals);
    }

    public object ActionProductForecastReport() {
        var action = this.Product.ActionProductForecastReport();
        action.Context = new {
            ActiveId = this.Product.Id,
            ActiveModel = "product.product",
            WarehouseId = this.Warehouse.Id
        };
        return action;
    }

    public object ActionOpenOrderpoints() {
        return GetOrderpointAction();
    }

    public object ActionStockReplenishmentInfo() {
        var action = env.IrActions.ForXmlId("stock.action_stock_replenishment_info");
        action.Name = $"Replenishment Information for {this.Product.DisplayName} in {this.Warehouse.DisplayName}";
        var res = env.StockReplenishmentInfo.Create(new {
            OrderpointId = this.Id
        });
        action.ResId = res.Id;
        return action;
    }

    public object ActionReplenish(bool forceToMax = false) {
        if (forceToMax) {
            this.QtyToOrder = this.ProductMaxQty - this.QtyForecast;
            var remainder = this.QtyMultiple > 0 && this.QtyToOrder % this.QtyMultiple != 0 ? this.QtyMultiple - (this.QtyToOrder % this.QtyMultiple) : 0.0;
            if (remainder != 0) {
                this.QtyToOrder += remainder;
            }
        }
        try {
            ProcureOrderpointConfirm();
        } catch (Exception e) {
            if (e.GetType() == typeof(Exception)) {
                throw;
            }
            throw new RedirectWarning(e, new {
                Name = this.Product.DisplayName,
                Type = "ir.actions.act_window",
                ResModel = "product.product",
                ResId = this.Product.Id,
                Views = new[] { new { 
                    Id = env.Ref("product.product_normal_form_view").Id, 
                    View = "form" 
                }}
            }, "Edit Product");
        }
        object notification = null;
        if (this.CreateUid.Id == (int)env.SuperuserId) {
            if (this.QtyToOrder <= 0 && this.Trigger == "Manual") {
                env.StockWarehouseOrderpoint.Unlink(this);
            }
            notification = GetReplenishmentOrderNotification();
        }
        ActionRemoveManualQtyToOrder();
        ComputeQtyToOrder();
        return notification;
    }

    public object ActionReplenishAuto() {
        this.Trigger = "Auto";
        return ActionReplenish();
    }

    public void ComputeQty() {
        if (this.Product == null || this.Location == null) {
            this.QtyOnHand = 0;
            this.QtyForecast = 0;
            return;
        }
        var orderpointContext = GetProductContext();
        var productsQty = this.Product.WithContext(orderpointContext).Read(new[] { "QtyAvailable", "VirtualAvailable" });
        var productsQtyInProgress = _QuantityInProgress();
        this.QtyOnHand = (float)productsQty[0]["QtyAvailable"];
        this.QtyForecast = (float)productsQty[0]["VirtualAvailable"] + productsQtyInProgress[this.Id];
    }

    public void ComputeQtyToOrder() {
        this.QtyToOrder = this.QtyToOrderManual != 0 ? this.QtyToOrderManual : this.QtyToOrderComputed;
    }

    public void InverseQtyToOrder() {
        this.QtyToOrderManual = this.QtyToOrder;
    }

    public object[] SearchQtyToOrder(string operator, object value) {
        var records = env.StockWarehouseOrderpoint.SearchFetch(new[] { "QtyToOrderManual", "in", new[] { 0, null } }, new[] { "QtyToOrderComputed" });
        var matchedIds = records.Where(r => (float)r["QtyToOrderComputed"] == (float)value).Select(r => r.Id).ToList();
        return new object[] { "|", "QtyToOrderManual", operator, value, "Id", "in", matchedIds };
    }

    public void ComputeQtyToOrderComputed() {
        if (this.Product == null || this.Location == null) {
            this.QtyToOrderComputed = 0;
            return;
        }
        var qtyToOrder = 0.0;
        var rounding = this.ProductUom.Rounding;
        if (this.QtyForecast < this.ProductMinQty) {
            var productContext = GetProductContext(this.VisibilityDays);
            var qtyForecastWithVisibility = this.Product.WithContext(productContext).Read(new[] { "VirtualAvailable" })[0]["VirtualAvailable"] + _QuantityInProgress()[this.Id];
            qtyToOrder = Math.Max(this.ProductMinQty, this.ProductMaxQty) - qtyForecastWithVisibility;
            var remainder = this.QtyMultiple > 0 && qtyToOrder % this.QtyMultiple != 0 ? this.QtyMultiple - (qtyToOrder % this.QtyMultiple) : 0.0;
            if (remainder > 0 && (this.QtyMultiple - remainder) > 0) {
                if (this.ProductMaxQty == 0) {
                    qtyToOrder += remainder;
                } else {
                    qtyToOrder -= remainder;
                }
            }
        }
        this.QtyToOrderComputed = qtyToOrder;
    }

    public float GetQtyMultipleToOrder() {
        return 0;
    }

    public void SetDefaultRouteId() {
        if (this.Route != null) {
            return;
        }
        var rulesGroups = env.StockRule.ReadGroup(new[] {
            "RouteId.ProductSelectable", "!=", false,
            "LocationDestId", "in", this.Location.Id,
            "Action", "in", new[] { "pull_push", "pull" },
            "RouteId.Active", "!=", false
        }, new[] { "LocationDestId", "RouteId" }, new[] { "Id:recordset" });
        foreach (var item in rulesGroups) {
            var locationDest = (StockLocation)item[0];
            var route = (StockRoute)item[1];
            var orderpoints = this.Where(o => o.Location.Id == locationDest.Id);
            orderpoints.Route = route;
        }
    }

    public object GetLeadDaysValues() {
        return new {
            DaysToOrder = this.DaysToOrder
        };
    }

    public object GetProductContext(float visibilityDays = 0) {
        return new {
            Location = this.Location.Id,
            ToDate = DateTime.Now.Date.AddDays(this.LeadDaysDate.Days + visibilityDays)
        };
    }

    public object GetOrderpointAction() {
        var action = env.IrActions.ForXmlId("stock.action_orderpoint_replenish");
        action.Context = env.Context;
        var orderpoints = env.StockWarehouseOrderpoint.Search(null, active_test: false);
        var orderpointsRemoved = _UnlinkProcessedOrderpoints();
        orderpoints = orderpoints.Except(orderpointsRemoved);
        var toRefill = new Dictionary<Tuple<int, int>, float>();
        var allProductIds = GetOrderpointProducts();
        var allReplenishLocationIds = GetOrderpointLocations();
        var plocPerDay = new Dictionary<Tuple<int, StockLocation>, List<int>>();
        var move = env.StockMove.Search(null, active_test: false);
        var quant = env.StockQuant.Search(null, active_test: false);
        var domainQuant = new[] { "Product", "in", allProductIds.Select(p => p.Id).ToList() };
        var domainMoveInLoc = new[] { "LocationDestId", "in", allReplenishLocationIds.Select(l => l.Id).ToList() };
        var domainMoveOutLoc = new[] { "LocationId", "in", allReplenishLocationIds.Select(l => l.Id).ToList() };
        var domainState = new[] { "State", "in", new[] { "waiting", "confirmed", "assigned", "partially_available" } };
        var domainProduct = new[] { "ProductId", "in", allProductIds.Select(p => p.Id).ToList() };
        domainQuant = new[] { domainProduct, domainQuant };
        var domainMoveIn = new[] { domainProduct, domainState, domainMoveInLoc };
        var domainMoveOut = new[] { domainProduct, domainState, domainMoveOutLoc };
        var movesIn = new Dictionary<int, List<Tuple<int, int, float>>>();
        foreach (var item in move.ReadGroup(domainMoveIn, new[] { "ProductId", "LocationDestId", "LocationFinalId" }, new[] { "ProductQty:sum" })) {
            if (!movesIn.ContainsKey((int)item[0])) {
                movesIn.Add((int)item[0], new List<Tuple<int, int, float>>());
            }
            movesIn[(int)item[0]].Add(new Tuple<int, int, float>((int)item[1], (int)item[2], (float)item[3]));
        }
        var movesOut = new Dictionary<int, List<Tuple<int, int>>>();
        foreach (var item in move.ReadGroup(domainMoveOut, new[] { "ProductId", "LocationId" }, new[] { "ProductQty:sum" })) {
            if (!movesOut.ContainsKey((int)item[0])) {
                movesOut.Add((int)item[0], new List<Tuple<int, int>>());
            }
            movesOut[(int)item[0]].Add(new Tuple<int, int>((int)item[1], (float)item[2]));
        }
        var quants = new Dictionary<int, List<Tuple<int, float>>>();
        foreach (var item in quant.ReadGroup(domainQuant, new[] { "ProductId", "LocationId" }, new[] { "Quantity:sum" })) {
            if (!quants.ContainsKey((int)item[0])) {
                quants.Add((int)item[0], new List<Tuple<int, float>>());
            }
            quants[(int)item[0]].Add(new Tuple<int, float>((int)item[1], (float)item[2]));
        }
        var rounding = allProductIds.ToDictionary(p => p.Id, p => p.UomId.Rounding);
        var path = env.StockLocation.Search(new[] { "Id", "child_of", allReplenishLocationIds.Select(l => l.Id).ToList() }).ToDictionary(loc => loc.Id, loc => loc.ParentPath);
        foreach (var loc in allReplenishLocationIds) {
            foreach (var product in allProductIds) {
                var qtyAvailable = quants.ContainsKey(product.Id) ? quants[product.Id].Where(q => IsParentPathIn(loc, path, (int)q.Item1)).Sum(q => q.Item2) : 0;
                var incomingQty = movesIn.ContainsKey(product.Id) ? movesIn[product.Id].Where(m => IsParentPathIn(loc, path, (int)m.Item1) || IsParentPathIn(loc, path, (int)m.Item2)).Sum(m => m.Item3) : 0;
                var outgoingQty = movesOut.ContainsKey(product.Id) ? movesOut[product.Id].Where(m => IsParentPathIn(loc, path, (int)m.Item1)).Sum(m => m.Item2) : 0;
                if (qtyAvailable + incomingQty - outgoingQty < 0) {
                    var rules = product.GetRulesFromLocation(loc);
                    var leadDays = rules.WithContext(bypass_delay_description: true).GetLeadDays(product).TotalDelay;
                    if (!plocPerDay.ContainsKey(new Tuple<int, StockLocation>((int)leadDays, loc))) {
                        plocPerDay.Add(new Tuple<int, StockLocation>((int)leadDays, loc), new List<int>());
                    }
                    plocPerDay[new Tuple<int, StockLocation>((int)leadDays, loc)].Add(product.Id);
                }
            }
        }
        var today = DateTime.Now.Date;
        foreach (var item in plocPerDay) {
            var products = env.ProductProduct.Browse(item.Value);
            var qties = products.WithContext(
                location: item.Key.Item2.Id,
                to_date: today.AddDays(item.Key.Item1)
            ).Read(new[] { "VirtualAvailable" });
            for (int i = 0; i < products.Count; i++) {
                var product = products[i];
                var qty = (float)qties[i]["VirtualAvailable"];
                if (qty < 0) {
                    if (!toRefill.ContainsKey(new Tuple<int, int>(product.Id, item.Key.Item2.Id))) {
                        toRefill.Add(new Tuple<int, int>(product.Id, item.Key.Item2.Id), qty);
                    }
                }
            }
        }
        if (toRefill.Count == 0) {
            return action;
        }
        var productIds = toRefill.Keys.Select(k => k.Item1).ToList();
        var locationIds = toRefill.Keys.Select(k => k.Item2).ToList();
        var qtyByProductLoc = env.ProductProduct.Browse(productIds).GetQuantityInProgress(locationIds: locationIds);
        var rounding = env.DecimalPrecision.PrecisionGet("Product Unit of Measure");
        var orderpointByProductLocation = env.StockWarehouseOrderpoint.ReadGroup(
            new[] { "Id", "in", orderpoints.Select(o => o.Id).ToList() },
            new[] { "ProductId", "LocationId" },
            new[] { "Id:recordset" });
        var orderpointByProductLocationDict = orderpointByProductLocation.ToDictionary(
            item => new Tuple<int, int>((int)item[0], (int)item[1]),
            item => (StockWarehouseOrderpoint)item[2]
        );
        foreach (var item in toRefill) {
            var product = item.Key.Item1;
            var locationId = item.Key.Item2;
            var productQty = item.Value;
            var qtyInProgress = qtyByProductLoc.ContainsKey(new Tuple<int, int>(product, locationId)) ? (float)qtyByProductLoc[new Tuple<int, int>(product, locationId)] : 0.0;
            qtyInProgress += orderpointByProductLocationDict.ContainsKey(new Tuple<int, int>(product, locationId)) ? orderpointByProductLocationDict[new Tuple<int, int>(product, locationId)].QtyToOrder : 0.0;
            if (qtyInProgress == 0) {
                continue;
            }
            toRefill[new Tuple<int, int>(product, locationId)] = productQty + qtyInProgress;
        }
        toRefill = toRefill.Where(kvp => kvp.Value < 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var orderpointByProductLocation2 = env.StockWarehouseOrderpoint.ReadGroup(
            new[] { "Id", "in", orderpoints.Select(o => o.Id).ToList() },
            new[] { "ProductId", "LocationId" },
            new[] { "Id:recordset" });
        var orderpointByProductLocationDict2 = orderpointByProductLocation2.ToDictionary(
            item => new Tuple<int, int>((int)item[0], (int)item[1]),
            item => (StockWarehouseOrderpoint)item[2]
        );
        var orderpointValuesList = new List<object>();
        foreach (var item in toRefill) {
            var product = item.Key.Item1;
            var locationId = item.Key.Item2;
            var productQty = item.Value;
            var orderpoint = orderpointByProductLocationDict2.ContainsKey(new Tuple<int, int>(product, locationId)) ? orderpointByProductLocationDict2[new Tuple<int, int>(product, locationId)] : null;
            if (orderpoint != null) {
                orderpoint.QtyForecast += productQty;
            } else {
                var orderpointValues = GetOrderpointValues(product, locationId);
                var location = env.StockLocation.Browse(locationId);
                orderpointValues = new {
                    Name = "Replenishment Report",
                    Warehouse = location.WarehouseId != null ? location.WarehouseId.Id : env.StockWarehouse.Search(new[] { "CompanyId", "=", location.CompanyId.Id }, limit: 1).Id,
                    CompanyId = location.CompanyId.Id,
                    orderpointValues
                };
                orderpointValuesList.Add(orderpointValues);
            }
        }
        orderpoints = env.StockWarehouseOrderpoint.WithUser((int)env.SuperuserId).Create(orderpointValuesList);
        foreach (var orderpoint in orderpoints) {
            orderpoint.SetDefaultRouteId();
            orderpoint.QtyMultiple = orderpoint.GetQtyMultipleToOrder();
        }
        return action;
    }

    public void ActionRemoveManualQtyToOrder() {
        this.QtyToOrderManual = 0;
    }

    public object GetOrderpointValues(ProductProduct product, int location) {
        return new {
            Product = product,
            Location = location,
            ProductMaxQty = 0.0,
            ProductMinQty = 0.0,
            Trigger = "Manual"
        };
    }

    public object GetReplenishmentOrderNotification() {
        var domain = new[] { "OrderpointId", "in", new[] { this.Id } };
        if (env.Context.ContainsKey("written_after")) {
            domain = new[] { domain, "WriteDate", ">=", env.Context["written_after"] };
        }
        var move = env.StockMove.Search(domain, limit: 1);
        if ((move.LocationId.WarehouseId != null && move.LocationId.WarehouseId != this.Warehouse) || move.LocationId.Usage == "transit" && move.PickingId != null) {
            var action = env.Ref("stock.stock_picking_action_picking_type");
            return new {
                Type = "ir.actions.client",
                Tag = "display_notification",
                Params = new {
                    Title = "The inter-warehouse transfers have been generated",
                    Message = move.PickingId.Name,
                    Links = new[] { new {
                        Label = move.PickingId.Name,
                        Url = $"/web#action={action.Id}&id={move.PickingId.Id}&model=stock.picking&view_type=form"
                    }},
                    Sticky = false
                }
            };
        }
        return null;
    }

    public Dictionary<int, float> _QuantityInProgress() {
        return new Dictionary<int, float>() {
            { this.Id, 0 }
        };
    }

    public List<StockWarehouseOrderpoint> _UnlinkProcessedOrderpoints() {
        var domain = new[] {
            "CreateUid", "=", env.SuperuserId,
            "Trigger", "=", "Manual",
            "QtyToOrder", "<=", 0
        };
        if (this.Id != 0) {
            domain = new[] { domain, "Id", "in", new[] { this.Id } };
        }
        var orderpointsToRemove = env.StockWarehouseOrderpoint.Search(domain, active_test: false);
        orderpointsToRemove.Unlink();
        return orderpointsToRemove;
    }

    public object _PrepareProcurementValues(DateTime date = default, ProcurementGroup group = null) {
        date = date != default ? date : DateTime.Now.Date;
        var datesInfo = this.Product.GetDatesInfo(date, this.Location, this.Route);
        return new {
            RouteIds = this.Route,
            DatePlanned = datesInfo.DatePlanned,
            DateOrder = datesInfo.DateOrder,
            DateDeadline = date,
            Warehouse = this.Warehouse,
            Orderpoint = this,
            GroupId = group ?? this.GroupId
        };
    }

    public object ProcureOrderpointConfirm(bool useNewCursor = false, ResCompany company = null, bool raiseUserError = true) {
        this.WithContext(company);
        foreach (var orderpointsBatchIds in Enumerable.Range(0, this.Id).Select((_, i) => new[] { i })) {
            if (useNewCursor) {
                // Implement custom logic for new cursor and auto-commit if needed
            }
            try {
                // Implement logic for processing orderpoints in batches and handling exceptions if needed
            } finally {
                if (useNewCursor) {
                    // Implement logic for committing and closing the cursor if needed
                }
            }
        }
        return new { };
    }

    public void _PostProcessScheduler() {
        // No implementation required
    }

    public DateTime _GetOrderpointProcurementDate() {
        return DateTime.Now;
    }

    public List<ProductProduct> GetOrderpointProducts() {
        return env.ProductProduct.Search(new[] { "IsStorable", "=", true, "StockMoveIds", "!=", false });
    }

    public List<StockLocation> GetOrderpointLocations() {
        return env.StockLocation.Search(new[] { "ReplenishLocation", "=", true });
    }

    private bool IsParentPathIn(StockLocation resupplyLoc, Dictionary<int, string> pathDict, int recordLoc) {
        return recordLoc != 0 && resupplyLoc.ParentPath.Contains(pathDict.ContainsKey(recordLoc) ? pathDict[recordLoc] : "");
    }
}
