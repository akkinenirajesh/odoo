csharp
public partial class SaleOrder
{
    public SaleOrderLine _cart_find_product_line(int productId = 0, int lineId = 0, int eventTicketId = 0)
    {
        var lines = Env.Call<SaleOrderLine>("_cart_find_product_line", new object[] { productId, lineId });
        if (lineId != 0 || eventTicketId == 0)
        {
            return lines;
        }

        return lines.Where(line => line.EventTicketId == eventTicketId).FirstOrDefault();
    }

    public Tuple<int, string> _verify_updated_quantity(SaleOrderLine orderLine, int productId, int newQty, int eventTicketId = 0)
    {
        var result = Env.Call<Tuple<int, string>>("_verify_updated_quantity", new object[] { orderLine, productId, newQty });
        if (eventTicketId == 0)
        {
            if (orderLine.EventTicketId == 0 || newQty < orderLine.ProductUomQty)
            {
                return result;
            }
            else
            {
                return new Tuple<int, string>(orderLine.ProductUomQty, Env.Translate("You cannot raise manually the event ticket quantity in your cart"));
            }
        }

        var ticket = Env.Ref<EventTicket>(eventTicketId);
        if (ticket == null)
        {
            throw new Exception(Env.Translate("The provided ticket doesn't exist"));
        }

        var existingQty = orderLine != null ? orderLine.ProductUomQty : 0;
        var qtyAdded = newQty - existingQty;
        string warning = "";

        if (ticket.SeatsLimited && ticket.SeatsAvailable <= 0)
        {
            newQty = existingQty;
            warning = Env.Translate("Sorry, The %(ticket)s tickets for the %(event)s event are sold out.", new { ticket = ticket.Name, event = ticket.EventId.Name });
        }
        else if (ticket.SeatsLimited && qtyAdded > ticket.SeatsAvailable)
        {
            newQty = existingQty + ticket.SeatsAvailable;
            warning = Env.Translate("Sorry, only %(remaining_seats)d seats are still available for the %(ticket)s ticket for the %(event)s event.", new { remaining_seats = ticket.SeatsAvailable, ticket = ticket.Name, event = ticket.EventId.Name });
        }

        return new Tuple<int, string>(newQty, warning);
    }

    public Dictionary<string, object> _prepare_order_line_values(int productId, int quantity, int eventTicketId = 0)
    {
        var values = Env.Call<Dictionary<string, object>>("_prepare_order_line_values", new object[] { productId, quantity });

        if (eventTicketId == 0)
        {
            return values;
        }

        var ticket = Env.Ref<EventTicket>(eventTicketId);
        if (ticket.ProductTemplateId.Id != productId)
        {
            throw new Exception(Env.Translate("The ticket doesn't match with this product."));
        }

        values["EventId"] = ticket.EventId.Id;
        values["EventTicketId"] = ticket.Id;

        return values;
    }

    public void _update_cart_line_values(SaleOrderLine orderLine, Dictionary<string, object> updateValues)
    {
        var oldQty = orderLine.ProductUomQty;

        Env.Call("_update_cart_line_values", new object[] { orderLine, updateValues });
        if (orderLine.EventTicketId == 0)
        {
            return;
        }

        var newQty = orderLine.ProductUomQty;
        if (newQty < oldQty)
        {
            var attendees = Env.Search<EventRegistration>(new object[] {
                new object[] {"State", "!=", "cancel"},
                new object[] {"SaleOrderId", "=", this.Id},
                new object[] {"EventTicketId", "=", orderLine.EventTicketId.Id},
            }, offset: newQty, limit: (oldQty - newQty), order: "Create_Date asc");
            attendees.Call("ActionCancel");
        }
    }
}

public partial class SaleOrderLine
{
    public void _compute_name_short()
    {
        Env.Call("_compute_name_short");
        if (EventTicketId != 0)
        {
            NameShort = EventTicketId.Name;
        }
    }
}
