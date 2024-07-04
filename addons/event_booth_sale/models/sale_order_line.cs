csharp
public partial class SaleOrderLine
{
    public bool ComputeIsEventBooth()
    {
        return this.Product.ServiceTracking == "event_booth";
    }

    public string ComputeNameShort()
    {
        if (this.EventBoothPendingIds.Any())
        {
            return this.EventBoothPendingIds.First().Event.Name;
        }
        // Call base implementation for other cases
        return base.ComputeNameShort();
    }

    public void InverseEventBoothPendingIds()
    {
        var existingBooths = this.EventBoothRegistrationIds.Select(r => r.EventBooth).ToList();
        var selectedBooths = this.EventBoothPendingIds.ToList();

        // Remove de-selected registrations
        this.EventBoothRegistrationIds
            .Where(reg => !selectedBooths.Contains(reg.EventBooth))
            .ToList()
            .ForEach(reg => Env.Remove(reg));

        // Create new registrations
        var newBooths = selectedBooths.Except(existingBooths);
        foreach (var booth in newBooths)
        {
            Env.Create<Event.BoothRegistration>(new {
                EventBooth = booth,
                SaleOrderLine = this,
                Partner = this.Order.Partner
            });
        }
    }

    public void CheckEventBoothRegistrationIds()
    {
        var eventIds = this.EventBoothRegistrationIds.Select(r => r.EventBooth.Event.Id).Distinct();
        if (eventIds.Count() > 1)
        {
            throw new ValidationException("Registrations from the same Order Line must belong to a single event.");
        }
    }

    public void OnChangeProductIdBooth()
    {
        if (this.Event != null && (this.Product == null || !this.EventBoothPendingIds.Any(b => b.Product == this.Product)))
        {
            this.Event = null;
        }
    }

    public void OnChangeEventIdBooth()
    {
        if (this.EventBoothPendingIds.Any() && (this.Event == null || this.Event != this.EventBoothPendingIds.First().Event))
        {
            this.EventBoothPendingIds.Clear();
        }
    }

    public bool UpdateEventBooths(bool setPaid = false)
    {
        if (this.IsEventBooth)
        {
            if (this.EventBoothPendingIds.Any() && !this.EventBoothIds.Any())
            {
                var unavailable = this.EventBoothPendingIds.Where(booth => !booth.IsAvailable).ToList();
                if (unavailable.Any())
                {
                    var boothNames = string.Join("\n\t- ", unavailable.Select(b => b.DisplayName));
                    throw new ValidationException($"The following booths are unavailable, please remove them to continue :\n\t- {boothNames}");
                }
                foreach (var registration in this.EventBoothRegistrationIds)
                {
                    registration.ActionConfirm();
                }
            }
            if (this.EventBoothIds.Any() && setPaid)
            {
                foreach (var booth in this.EventBoothIds)
                {
                    booth.ActionSetPaid();
                }
            }
        }
        return true;
    }

    public string GetSaleOrderLineMultilineDescriptionSale()
    {
        if (this.EventBoothPendingIds.Any())
        {
            return string.Join("\n", this.EventBoothPendingIds.Select(b => b.GetBoothMultilineDescription()));
        }
        return base.GetSaleOrderLineMultilineDescriptionSale();
    }

    public bool UseTemplateName()
    {
        if (this.EventBoothPendingIds.Any())
        {
            return false;
        }
        return base.UseTemplateName();
    }

    public decimal GetDisplayPrice()
    {
        if (this.EventBoothPendingIds.Any() && this.Event != null)
        {
            var company = this.Event.Company ?? Env.Company;
            var pricelist = this.Order.Pricelist;
            decimal totalPrice;

            if (pricelist.DiscountPolicy == "with_discount")
            {
                var eventBooths = this.EventBoothPendingIds.WithContext(GetPricelistPriceContext());
                totalPrice = eventBooths.Sum(booth => booth.BoothCategory.PriceReduce);
            }
            else
            {
                totalPrice = this.EventBoothPendingIds.Sum(booth => booth.Price);
            }

            return ConvertToSolCurrency(totalPrice, company.Currency);
        }
        return base.GetDisplayPrice();
    }
}
