C#
public partial class SaleOrderLine {

    public void ComputeWarehouseId()
    {
        this.Warehouse = Env.Get<SaleOrder>().FindOne(this.OrderId).Warehouse;
        if (this.Route != null)
        {
            var domain = new[] {
                $"location_dest_id = {Env.Get<ResPartner>().FindOne(this.Order.PartnerShipping.Id).PropertyStockCustomer.Id}",
                $"action != 'push'"
            };
            var rules = Env.Get<StockRule>().Search(
                domain,
                $"route_sequence, sequence"
            );
            if (rules.Count > 0)
            {
                this.Warehouse = Env.Get<StockLocation>().FindOne(rules[0].LocationSrc.Id).Warehouse;
            }
        }
    }

    public void ComputeQtyToDeliver()
    {
        this.QtyToDeliver = this.ProductUomQty - this.QtyDelivered;
        if (this.State in new[] {"draft", "sent", "sale"} && this.IsStorable && this.ProductUom != null && this.QtyToDeliver > 0)
        {
            if (this.State == "sale" && this.MoveIds.Count == 0)
            {
                this.DisplayQtyWidget = false;
            }
            else
            {
                this.DisplayQtyWidget = true;
            }
        }
        else
        {
            this.DisplayQtyWidget = false;
        }
    }

    public void ComputeQtyAtDate()
    {
        if (this.State == "sale")
        {
            if (!this.DisplayQtyWidget)
            {
                return;
            }
            var moves = Env.Get<StockMove>().Search(
                new [] {
                    $"sale_line_id = {this.Id}",
                    $"product_id = {this.Product.Id}"
                }
            );
            this.ForecastExpectedDate = moves.Max(m => m.ForecastExpectedDate);
            this.QtyAvailableToday = 0;
            this.FreeQtyToday = 0;
            foreach (var move in moves)
            {
                this.QtyAvailableToday += move.ProductUom.ComputeQuantity(move.Quantity, this.ProductUom);
                this.FreeQtyToday += move.Product.Uom.ComputeQuantity(move.ForecastAvailability, this.ProductUom);
            }
            this.ScheduledDate = this.Order.CommitmentDate ?? ExpectedDate();
            this.VirtualAvailableAtDate = 0;
        }
        else if (this.State in new[] {"draft", "sent"})
        {
            var lines = Env.Get<SaleOrderLine>().Search(
                new [] {
                    $"warehouse_id = {this.Warehouse.Id}",
                    $"order_id.commitment_date = {this.Order.CommitmentDate ?? ExpectedDate()}"
                }
            );
            foreach (var line in lines)
            {
                var productQties = Env.Get<ProductProduct>().Search(
                    new[] {
                        $"id = {line.Product.Id}"
                    }
                ).Read(new [] { "QtyAvailable", "FreeQty", "VirtualAvailable" }, new { to_date = this.ScheduledDate, warehouse_id = this.Warehouse.Id });
                var qtyAvailableToday = productQties[0].QtyAvailable;
                var freeQtyToday = productQties[0].FreeQty;
                var virtualAvailableAtDate = productQties[0].VirtualAvailable;
                line.QtyAvailableToday = qtyAvailableToday;
                line.FreeQtyToday = freeQtyToday;
                line.VirtualAvailableAtDate = virtualAvailableAtDate;
                line.ForecastExpectedDate = null;
            }
        }
    }

    public void ComputeIsMTO()
    {
        if (!this.DisplayQtyWidget)
        {
            return;
        }
        var productRoutes = this.Route != null ? new [] { this.Route } : this.Product.RouteIds.Concat(this.Product.CategId.TotalRouteIds).ToArray();
        var mtoRoute = Env.Get<StockWarehouse>().FindOne(this.Order.Warehouse.Id).MtoPull.Route;
        if (mtoRoute == null)
        {
            mtoRoute = Env.Get<StockWarehouse>().FindOrCreateGlobalRoute("stock.route_warehouse0_mto", "Replenish on Order (MTO)");
        }
        this.IsMTO = productRoutes.Contains(mtoRoute);
    }

    public void ComputeQtyDeliveredMethod()
    {
        if (!this.IsExpense && this.Product.Type == "consu")
        {
            this.QtyDeliveredMethod = "StockMove";
        }
    }

    public void ComputeQtyDelivered()
    {
        if (this.QtyDeliveredMethod == "StockMove")
        {
            var outgoingMoves = Env.Get<StockMove>().Search(new [] {
                $"sale_line_id = {this.Id}",
                $"state != 'done'",
                $"scrapped = false",
                $"product_id = {this.Product.Id}",
                $"location_dest_id.usage = 'customer'"
            });
            var incomingMoves = Env.Get<StockMove>().Search(new [] {
                $"sale_line_id = {this.Id}",
                $"state != 'done'",
                $"scrapped = false",
                $"product_id = {this.Product.Id}",
                $"location_dest_id.usage != 'customer'"
            });
            this.QtyDelivered = outgoingMoves.Sum(m => m.ProductUom.ComputeQuantity(m.Quantity, this.ProductUom));
            this.QtyDelivered -= incomingMoves.Sum(m => m.ProductUom.ComputeQuantity(m.Quantity, this.ProductUom));
        }
    }

    public void ComputeProductUpdatable()
    {
        if (this.MoveIds.Any(m => m.State != "cancel"))
        {
            this.ProductUpdatable = false;
        }
    }

    public void ComputeCustomerLead()
    {
        this.CustomerLead = this.Product.SaleDelay;
    }

    public void InverseCustomerLead()
    {
        if (this.State == "sale" && this.Order.CommitmentDate == null)
        {
            foreach (var move in this.MoveIds)
            {
                move.DateDeadline = this.Order.DateOrder.AddDays(this.CustomerLead);
            }
        }
    }

    private DateTime ExpectedDate()
    {
        return this.Order.CommitmentDate ?? this.Order.DateOrder.AddDays(this.Order.Company.SecurityLead);
    }

}
