csharp
public partial class PurchaseStock.SupplierInfo
{
    public DateTime LastPurchaseDate { get; set; }
    public bool ShowSetSupplierButton { get; set; }

    public void ComputeLastPurchaseDate()
    {
        LastPurchaseDate = null;
        var purchases = Env.Search<Purchase.PurchaseOrder>(new[] { 
            new SearchTerm { Field = "State", Operator = "in", Values = new[] { "purchase", "done" } },
            new SearchTerm { Field = "OrderLine.ProductId", Operator = "in", Values = this.ProductTmplId.ProductVariantIds.Ids }
        }, new[] { new OrderBy { Field = "DateOrder", Order = OrderDirection.Descending } });
        foreach (var purchase in purchases)
        {
            if (purchase.PartnerId == this.PartnerId)
            {
                LastPurchaseDate = purchase.DateOrder;
                break;
            }
        }
    }

    public void ComputeShowSetSupplierButton()
    {
        ShowSetSupplierButton = true;
        var orderpointId = Env.Context.GetOrDefault("default_orderpoint_id");
        var orderpoint = Env.Get<Stock.WarehouseOrderpoint>(orderpointId);
        if (orderpointId != null)
        {
            if (this.Id == orderpoint.SupplierId.Id)
            {
                ShowSetSupplierButton = false;
            }
        }
    }

    public void ActionSetSupplier()
    {
        var orderpointId = Env.Context.GetOrDefault("orderpoint_id");
        var orderpoint = Env.Get<Stock.WarehouseOrderpoint>(orderpointId);
        if (orderpoint != null)
        {
            var buyRoute = Env.Search<Stock.Rule>(new[] { new SearchTerm { Field = "Action", Operator = "=", Values = new[] { "buy" } } }, limit: 1).FirstOrDefault();
            if (buyRoute != null)
            {
                orderpoint.RouteId = buyRoute.RouteId.Id;
            }
            orderpoint.SupplierId = this;
            var supplierMinQty = this.ProductUom.ComputeQuantity(this.MinQty, orderpoint.ProductId.UomId, round: false);
            if (orderpoint.QtyToOrder < supplierMinQty)
            {
                orderpoint.QtyToOrder = supplierMinQty;
            }
            var replenishId = Env.Context.GetOrDefault("replenish_id");
            if (replenishId != null)
            {
                var replenish = Env.Get<Product.Replenish>(replenishId);
                replenish.SupplierId = this;
                Env.Action<Product.Replenish>(replenish, "Form");
            }
        }
    }
}

public partial class PurchaseStock.ProductTemplate
{
    public int[] RouteIds { get; set; }

    public int[] GetBuyRoute()
    {
        var buyRoute = Env.Ref<Stock.Route>("purchase_stock.route_warehouse0_buy");
        if (buyRoute != null)
        {
            return new int[] { buyRoute.Id };
        }
        return new int[] { };
    }
}

public partial class PurchaseStock.ProductProduct
{
    public Purchase.PurchaseOrderLine[] PurchaseOrderLineIds { get; set; }

    public Tuple<Dictionary<Tuple<int, int>, decimal>, Dictionary<Tuple<int, int>, decimal>> GetQuantityInProgress(int[] locationIds = null, int[] warehouseIds = null)
    {
        if (locationIds == null)
        {
            locationIds = new int[] { };
        }
        if (warehouseIds == null)
        {
            warehouseIds = new int[] { };
        }
        var qtyByProductLocation = new Dictionary<Tuple<int, int>, decimal>();
        var qtyByProductWh = new Dictionary<Tuple<int, int>, decimal>();
        // Call parent _get_quantity_in_progress.
        var result = base.GetQuantityInProgress(locationIds, warehouseIds);
        qtyByProductLocation = result.Item1;
        qtyByProductWh = result.Item2;
        var domain = this.GetLinesDomain(locationIds, warehouseIds);
        var groups = Env.ReadGroup<Purchase.PurchaseOrderLine>(domain,
            new[] { "Order.Id", "ProductId", "ProductUom", "OrderpointId", "LocationFinalId" },
            new[] { new GroupBy { Field = "ProductQty", Aggregate = AggregateFunction.Sum } });
        foreach (var group in groups)
        {
            int order = (int)group[0];
            int product = (int)group[1];
            int uom = (int)group[2];
            int orderpoint = (int)group[3];
            int locationFinal = (int)group[4];
            decimal productQtySum = (decimal)group[5];
            int location;
            if (orderpoint != 0)
            {
                location = Env.Get<Stock.WarehouseOrderpoint>(orderpoint).LocationId.Id;
            }
            else if (locationFinal != 0)
            {
                location = locationFinal;
            }
            else
            {
                location = Env.Get<Purchase.PurchaseOrder>(order).PickingTypeId.DefaultLocationDestId.Id;
            }
            var productQty = Env.Get<Stock.Uom>(uom).ComputeQuantity(productQtySum, Env.Get<Product.Product>(product).UomId, round: false);
            qtyByProductLocation[new Tuple<int, int>(product, location)] += productQty;
            qtyByProductWh[new Tuple<int, int>(product, location)] += productQty;
        }
        return new Tuple<Dictionary<Tuple<int, int>, decimal>, Dictionary<Tuple<int, int>, decimal>>(qtyByProductLocation, qtyByProductWh);
    }

    private int[] GetLinesDomain(int[] locationIds = null, int[] warehouseIds = null)
    {
        var domains = new List<int[]>();
        var rfqDomain = new[] {
            new SearchTerm { Field = "State", Operator = "in", Values = new[] { "draft", "sent", "to approve" } },
            new SearchTerm { Field = "ProductId", Operator = "in", Values = this.Ids }
        };
        if (locationIds != null && locationIds.Length > 0)
        {
            domains.Add(new[] {
                new SearchTerm { Field = "Order.PickingTypeId.DefaultLocationDestId", Operator = "in", Values = locationIds },
                new SearchTerm { Field = "MoveIds", Operator = "=", Values = new[] { 0 } },
                new SearchTerm { Field = "LocationFinalId", Operator = "child_of", Values = locationIds },
                new SearchTerm { Field = "MoveDestIds", Operator = "=", Values = new[] { 0 } },
                new SearchTerm { Field = "OrderpointId.LocationId", Operator = "in", Values = locationIds }
            });
        }
        if (warehouseIds != null && warehouseIds.Length > 0)
        {
            domains.Add(new[] {
                new SearchTerm { Field = "Order.PickingTypeId.WarehouseId", Operator = "in", Values = warehouseIds },
                new SearchTerm { Field = "MoveDestIds", Operator = "=", Values = new[] { 0 } },
                new SearchTerm { Field = "OrderpointId.WarehouseId", Operator = "in", Values = warehouseIds }
            });
        }
        if (domains.Count > 0)
        {
            return domains.Aggregate((a, b) => new[] { new SearchTerm { Field = "OR", Operator = "OR", Values = new[] { a, b } } });
        }
        return new int[] { };
    }
}

public partial class PurchaseStock.ProductCategory
{
    public Account.Account PropertyAccountCreditorPriceDifferenceCateg { get; set; }
}

public partial class PurchaseStock.ProductTemplate
{
    public Account.Account PropertyAccountCreditorPriceDifference { get; set; }
}
