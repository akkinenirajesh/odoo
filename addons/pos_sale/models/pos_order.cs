csharp
public partial class PosOrder {
    public void CountSaleOrder() {
        this.SaleOrderCount = Env.GetRecords<PosOrderLine>(x => x.OrderId == this.Id).Count(x => x.SaleOrderOriginId != null);
    }

    public void ComputeCurrencyRate() {
        var dateOrder = this.DateOrder != null ? this.DateOrder : Env.Now();
        this.CurrencyRate = Env.GetRecord<ResCurrency>().GetConversionRate(this.CompanyId.CurrencyId, this.CurrencyId, this.CompanyId, dateOrder);
    }

    public void CompleteValuesFromSession(PosSession session, dynamic values) {
        values["CrmTeamId"] = session.ConfigId.CrmTeamId?.Id;
    }

    public void PrepareInvoiceVals() {
        var invoiceVals = base.PrepareInvoiceVals();
        invoiceVals["TeamId"] = this.CrmTeamId?.Id;
        var saleOrders = this.Lines.Where(x => x.SaleOrderOriginId != null).Select(x => x.SaleOrderOriginId).ToList();
        if (saleOrders.Any()) {
            if (saleOrders[0].PartnerInvoiceId != saleOrders[0].PartnerShippingId) {
                invoiceVals["PartnerShippingId"] = saleOrders[0].PartnerShippingId?.Id;
            }
            else {
                var addr = this.PartnerId.AddressGet("delivery");
                invoiceVals["PartnerShippingId"] = addr["delivery"];
            }
            if (saleOrders[0].PaymentTermId != null) {
                invoiceVals["InvoicePaymentTermId"] = saleOrders[0].PaymentTermId.Id;
            }
            if (saleOrders[0].PartnerInvoiceId != saleOrders[0].PartnerId) {
                invoiceVals["PartnerId"] = saleOrders[0].PartnerInvoiceId?.Id;
            }
        }
        return invoiceVals;
    }

    public dynamic SyncFromUi(dynamic orders) {
        dynamic data = base.SyncFromUi(orders);
        if (orders.Count == 0) {
            return data;
        }

        var orderIds = Env.GetRecords<PosOrder>(x => orders.Select(o => o.Id).Contains(x.Id));
        foreach (var order in orderIds) {
            foreach (var line in order.Lines.Where(l => l.ProductId == order.ConfigId.DownPaymentProductId && l.Quantity != 0 && (l.SaleOrderOriginId != null || (l.RefundedOrderlineId != null && l.RefundedOrderlineId.SaleOrderOriginId != null)))) {
                var saleLines = line.SaleOrderOriginId != null ? line.SaleOrderOriginId.OrderLine : (line.RefundedOrderlineId != null ? line.RefundedOrderlineId.SaleOrderOriginId.OrderLine : null);
                var saleOrderOrigin = line.SaleOrderOriginId != null ? line.SaleOrderOriginId : (line.RefundedOrderlineId != null ? line.RefundedOrderlineId.SaleOrderOriginId : null);
                if (!saleLines.Any(line => line.DisplayType != null && line.IsDownpayment)) {
                    var saleAdvancePaymentInv = Env.GetRecord<SaleAdvancePaymentInv>();
                    Env.GetRecords<SaleOrderLine>().Create(saleAdvancePaymentInv.PrepareDownPaymentSectionValues(saleOrderOrigin));
                }
                var orderReference = line.Name;
                var saleOrderLineDescription = $"Down payment (ref: {orderReference} on \n {line.Order.DateOrder:MM-dd-yy})";
                var saleLine = Env.GetRecords<SaleOrderLine>().Create(new {
                    OrderId = saleOrderOrigin.Id,
                    ProductId = line.ProductId.Id,
                    PriceUnit = line.PriceUnit,
                    ProductUomQty = 0,
                    TaxId = line.TaxIds.Select(x => x.Id).ToList(),
                    IsDownpayment = true,
                    Discount = line.Discount,
                    Sequence = saleLines.Any() ? saleLines.LastOrDefault().Sequence + 2 : 10,
                    Name = saleOrderLineDescription
                });
                line.SaleOrderLineId = saleLine;
            }
            var soLines = order.Lines.Where(x => x.SaleOrderLineId != null).Select(x => x.SaleOrderLineId).ToList();

            if (order.State != "draft") {
                var saleOrders = soLines.Select(x => x.OrderId).Distinct().ToList();
                foreach (var saleOrder in Env.GetRecords<SaleOrder>(x => saleOrders.Contains(x.Id)).Where(so => so.State == "draft" || so.State == "sent")) {
                    saleOrder.ActionConfirm();
                }
            }
            soLines.ForEach(x => x.FlushRecordset("QtyDelivered"));

            var waitingPickingIds = new HashSet<int>();
            foreach (var soLine in soLines) {
                var soLineStockMoveIds = soLine.MoveIds.Group.StockMoveIds;
                foreach (var stockMove in soLine.MoveIds) {
                    var picking = stockMove.PickingId;
                    if (picking.State != "waiting" && picking.State != "confirmed" && picking.State != "assigned") {
                        continue;
                    }
                    var newQty = soLine.ProductUomQty - soLine.QtyDelivered;
                    newQty = newQty <= 0 ? 0 : newQty;
                    stockMove.ProductUomQty = soLine.ComputeUomQty(newQty, stockMove, false);

                    foreach (var move in soLineStockMoveIds.Where(m => m.State == "waiting" || m.State == "confirmed" && m.ProductId == stockMove.ProductId)) {
                        move.ProductUomQty = stockMove.ProductUomQty;
                        waitingPickingIds.Add(move.PickingId.Id);
                    }
                    waitingPickingIds.Add(picking.Id);
                }
            }

            foreach (var picking in Env.GetRecords<StockPicking>(x => waitingPickingIds.Contains(x.Id))) {
                if (picking.MoveIds.All(move => move.ProductUomQty == 0)) {
                    picking.ActionCancel();
                }
            }
        }
        return data;
    }

    public void ActionViewSaleOrder() {
        var linkedOrders = this.Lines.Where(x => x.SaleOrderOriginId != null).Select(x => x.SaleOrderOriginId).ToList();
        Env.ActionWindow("sale.order", "tree,form", linkedOrders.Select(x => x.Id).ToList(), "Linked Sale Orders");
    }

    public List<string> GetFieldsForOrderLine() {
        var fields = base.GetFieldsForOrderLine();
        fields.AddRange(new List<string> {
            "SaleOrderOriginId",
            "DownPaymentDetails",
            "SaleOrderLineId"
        });
        return fields;
    }

    public dynamic PrepareOrderLine(dynamic orderLine) {
        orderLine = base.PrepareOrderLine(orderLine);
        if (orderLine.SaleOrderOriginId != null) {
            orderLine.SaleOrderOriginId = new {
                Id = orderLine.SaleOrderOriginId[0],
                Name = orderLine.SaleOrderOriginId[1]
            };
        }
        if (orderLine.SaleOrderLineId != null) {
            orderLine.SaleOrderLineId = new {
                Id = orderLine.SaleOrderLineId[0]
            };
        }
        return orderLine;
    }

    public dynamic GetInvoiceLinesValues(dynamic lineValues, PosOrderLine posLine) {
        var invLineVals = base.GetInvoiceLinesValues(lineValues, posLine);
        if (posLine.SaleOrderOriginId != null) {
            var originLine = posLine.SaleOrderLineId;
            originLine.SetAnalyticDistribution(invLineVals);
        }
        return invLineVals;
    }
}

public partial class PosOrderLine {
    public void ComputeQtyDelivered() {
        if (this.Order.State == "paid" || this.Order.State == "done" || this.Order.State == "invoiced") {
            var outgoingPickings = this.Order.PickingIds.Where(pick => pick.State == "done" && pick.PickingTypeCode == "outgoing").ToList();
            if (outgoingPickings.Any()) {
                var moves = outgoingPickings.SelectMany(x => x.MoveIds).Where(m => m.State == "done" && m.ProductId == this.ProductId).ToList();
                this.QtyDelivered = moves.Sum(x => x.Quantity);
            }
            else {
                this.QtyDelivered = 0;
            }
        }
    }

    public List<string> LoadPosDataFields(PosConfig configId) {
        var paramsList = base.LoadPosDataFields(configId);
        paramsList.AddRange(new List<string> {
            "SaleOrderOriginId",
            "SaleOrderLineId",
            "DownPaymentDetails"
        });
        return paramsList;
    }
}
