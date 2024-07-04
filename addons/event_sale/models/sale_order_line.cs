csharp
public partial class SaleOrderLine
{
    public void CheckEventRegistrationTicket()
    {
        if (ProductId.ServiceTracking == "event" && (EventId == null || EventTicketId == null))
        {
            throw new ValidationException($"The sale order line with the product {ProductId.Name} needs an event and a ticket.");
        }
    }

    public void ComputeProductUomReadonly()
    {
        if (EventId != null)
        {
            ProductUomReadonly = true;
        }
        else
        {
            base.ComputeProductUomReadonly();
        }
    }

    public bool InitRegistrations()
    {
        var registrationsVals = new List<Dictionary<string, object>>();
        if (ServiceTracking != "event")
        {
            return true;
        }

        for (int i = 0; i < (int)ProductUomQty - Registrations.Count(); i++)
        {
            registrationsVals.Add(new Dictionary<string, object>
            {
                { "SaleOrderLineId", Id },
                { "SaleOrderId", OrderId.Id }
            });
        }

        if (registrationsVals.Any())
        {
            Env.Get<Events.Registration>().Sudo().Create(registrationsVals);
        }
        return true;
    }

    public void ComputeEventId()
    {
        if (ProductId != null && ProductId.ServiceTracking == "event")
        {
            if (!EventId.EventTickets.Select(t => t.ProductId).Contains(ProductId))
            {
                EventId = null;
            }
        }
        else
        {
            EventId = null;
        }
    }

    public void ComputeEventTicketId()
    {
        if (EventId != null)
        {
            if (EventId != EventTicketId?.EventId)
            {
                EventTicketId = null;
            }
        }
        else
        {
            EventTicketId = null;
        }
    }

    public override void ComputePriceUnit()
    {
        base.ComputePriceUnit();
    }

    public override void ComputeName()
    {
        base.ComputeName();
    }

    public override string GetSaleOrderLineMultilineDescriptionSale()
    {
        if (EventTicketId != null)
        {
            return EventTicketId.GetTicketMultilineDescription() + GetSaleOrderLineMultilineDescriptionVariants();
        }
        else
        {
            return base.GetSaleOrderLineMultilineDescriptionSale();
        }
    }

    public override bool UseTemplateName()
    {
        if (EventTicketId != null)
        {
            return false;
        }
        return base.UseTemplateName();
    }

    public override decimal GetDisplayPrice()
    {
        if (EventTicketId != null && EventId != null)
        {
            var eventTicket = EventTicketId;
            var company = eventTicket.CompanyId ?? Env.Company;
            var pricelist = OrderId.PricelistId;
            decimal price;
            if (pricelist.DiscountPolicy == "with_discount")
            {
                price = eventTicket.WithContext(GetPricelistPriceContext()).PriceReduce;
            }
            else
            {
                price = eventTicket.Price;
            }
            return ConvertToSolCurrency(price, company.CurrencyId);
        }
        return base.GetDisplayPrice();
    }
}
