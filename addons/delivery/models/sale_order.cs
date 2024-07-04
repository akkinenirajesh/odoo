csharp
public partial class SaleOrder
{
    public decimal ComputeAmountTotalWithoutDelivery()
    {
        var deliveryCost = OrderLine.Where(l => l.IsDelivery).Sum(l => l.PriceTotal);
        return AmountTotal - deliveryCost;
    }

    public void ComputeDeliveryState()
    {
        DeliverySet = OrderLine.Any(line => line.IsDelivery);
    }

    public void OnChangeOrderLine()
    {
        var deliveryLine = OrderLine.FirstOrDefault(l => l.IsDelivery);
        if (deliveryLine != null)
        {
            RecomputeDeliveryPrice = true;
        }
    }

    public IEnumerable<SaleOrderLine> GetUpdatePricesLines()
    {
        var lines = base.GetUpdatePricesLines();
        return lines.Where(line => !line.IsDelivery);
    }

    public void RemoveDeliveryLine()
    {
        var deliveryLines = OrderLine.Where(l => l.IsDelivery).ToList();
        if (!deliveryLines.Any()) return;

        var toDelete = deliveryLines.Where(x => x.QtyInvoiced == 0).ToList();
        if (!toDelete.Any())
        {
            throw new UserException("You cannot update the shipping costs on an order where it was already invoiced!\n\n" +
                "The following delivery lines (product, invoiced quantity and price) have already been processed:\n\n" +
                string.Join("\n", deliveryLines.Select(line => 
                    $"- {line.ProductId.DisplayName}: {line.QtyInvoiced} x {line.PriceUnit}")));
        }

        foreach (var line in toDelete)
        {
            OrderLine.Remove(line);
        }
    }

    public bool SetDeliveryLine(DeliveryCarrier carrier, decimal amount)
    {
        RemoveDeliveryLine();
        CarrierId = carrier;
        CreateDeliveryLine(carrier, amount);
        return true;
    }

    public ActionResult ActionOpenDeliveryWizard()
    {
        var viewId = Env.Ref("delivery.choose_delivery_carrier_view_form").Id;
        var name = Env.Context.GetValueOrDefault("carrier_recompute", false) ? "Update shipping cost" : "Add a shipping method";
        var carrier = Env.Context.GetValueOrDefault("carrier_recompute", false) 
            ? CarrierId 
            : PartnerShippingId.PropertyDeliveryCarrierId ?? PartnerShippingId.CommercialPartnerId.PropertyDeliveryCarrierId;

        return new ActionResult
        {
            Name = name,
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "choose.delivery.carrier",
            ViewId = viewId,
            Views = new List<object[]> { new object[] { viewId, "form" } },
            Target = "new",
            Context = new Dictionary<string, object>
            {
                { "default_order_id", Id },
                { "default_carrier_id", carrier?.Id },
                { "default_total_weight", GetEstimatedWeight() }
            }
        };
    }

    private SaleOrderLineValues PrepareDeliveryLineVals(DeliveryCarrier carrier, decimal priceUnit)
    {
        // Implementation details...
    }

    private void CreateDeliveryLine(DeliveryCarrier carrier, decimal priceUnit)
    {
        var values = PrepareDeliveryLineVals(carrier, priceUnit);
        Env.Get<SaleOrderLine>().Create(values);
    }

    public void ComputeShippingWeight()
    {
        ShippingWeight = GetEstimatedWeight();
    }

    private float GetEstimatedWeight()
    {
        return OrderLine
            .Where(l => l.ProductId.Type == "consu" && !l.IsDelivery && !l.DisplayType && l.ProductUomQty > 0)
            .Sum(l => l.ProductQty * l.ProductId.Weight);
    }

    public decimal UpdateOrderLineInfo(int productId, decimal quantity, Dictionary<string, object> kwargs)
    {
        var priceUnit = base.UpdateOrderLineInfo(productId, quantity, kwargs);
        OnChangeOrderLine();
        return priceUnit;
    }
}
