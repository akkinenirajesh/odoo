C#
public partial class SaleOrder
{
    public virtual object SetDeliveryLine(Carrier carrier, decimal amount)
    {
        var res = Env.Call("super", "set_delivery_line", carrier, amount);

        foreach (var order in this)
        {
            if (order.State != "sale")
            {
                continue;
            }

            var pendingDeliveries = order.PickingIds.Where(p => p.State != "done" && p.State != "cancel" && !p.MoveIds.Any(m => m.OriginReturnedMoveId != null));
            foreach (var delivery in pendingDeliveries)
            {
                delivery.CarrierId = carrier.Id;
            }
        }
        return res;
    }

    public virtual SaleOrderLine CreateDeliveryLine(Carrier carrier, decimal priceUnit)
    {
        var sol = Env.Call("super", "_create_delivery_line", carrier, priceUnit);
        var context = new Dictionary<string, object>();
        if (this.PartnerId != null)
        {
            context["lang"] = this.PartnerId.Lang;
        }

        if (carrier.InvoicePolicy == "real")
        {
            sol.PriceUnit = 0;
            sol.Name = sol.Name + $" (Estimated Cost: {FormatCurrencyAmount(priceUnit)})";
        }

        return sol;
    }

    public virtual string FormatCurrencyAmount(decimal amount)
    {
        var pre = "";
        var post = "";
        if (this.CurrencyId.Position == "before")
        {
            pre = $"{this.CurrencyId.Symbol ?? ""} ";
        }
        else
        {
            post = $" {this.CurrencyId.Symbol ?? ""}";
        }

        return $" {pre}{amount}{post}";
    }
}

public partial class SaleOrderLine
{
    public virtual Dictionary<string, object> PrepareProcurementValues(int groupId)
    {
        var values = Env.Call("super", "_prepare_procurement_values", groupId);
        if (values.ContainsKey("RouteIds") && this.OrderId.CarrierId.RouteIds != null)
        {
            values["RouteIds"] = this.OrderId.CarrierId.RouteIds;
        }

        return values;
    }
}
