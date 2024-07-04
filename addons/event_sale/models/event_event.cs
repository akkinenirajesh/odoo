csharp
public partial class Event
{
    public decimal ComputeSalePriceSubtotal()
    {
        var dateNow = DateTime.Now;
        var eventSubtotals = Env.SaleOrderLine.ReadGroup(
            new[] {
                ("Event", "=", this.Id),
                ("PriceSubtotal", "!=", 0),
                ("State", "!=", "cancel")
            },
            new[] { "Event", "Currency" },
            new[] { "PriceSubtotal:sum" }
        );

        decimal totalSubtotal = 0;
        foreach (var group in eventSubtotals)
        {
            var eventId = group["Event"];
            var currencyId = group["Currency"];
            var sumPriceSubtotal = (decimal)group["PriceSubtotal:sum"];

            var event = Env.Event.Browse(eventId);
            var currency = Env.Currency.Browse(currencyId);

            totalSubtotal += event.Currency.Convert(
                sumPriceSubtotal,
                currency,
                event.Company ?? Env.Company,
                dateNow
            );
        }

        return totalSubtotal;
    }

    public Dictionary<string, object> ActionViewLinkedOrders()
    {
        var saleOrderAction = Env.IrActionsActions.ForXmlId("Sale.ActionOrders");
        saleOrderAction["Domain"] = new List<object>
        {
            new List<object> { "State", "!=", "cancel" },
            new List<object> { "OrderLine.Event", "=", this.Id }
        };
        saleOrderAction["Context"] = new Dictionary<string, object> { { "Create", 0 } };

        return saleOrderAction;
    }
}
