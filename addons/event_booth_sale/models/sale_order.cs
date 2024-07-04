csharp
public partial class SaleOrder
{
    public void ComputeEventBoothCount()
    {
        var slotData = Env.Get<Event.Booth>().ReadGroup(
            new[] { ("SaleOrderId", "in", new[] { this.Id }) },
            new[] { "SaleOrderId" },
            new[] { "__count" }
        );
        var slotMapped = slotData.ToDictionary(x => x.SaleOrderId, x => x.__count);
        this.EventBoothCount = slotMapped.GetValueOrDefault(this.Id, 0);
    }

    public override ActionResult ActionConfirm()
    {
        var res = base.ActionConfirm();

        if (!this.OrderLine.Any(line => line.ServiceTracking == "event_booth"))
        {
            return res;
        }

        var soLinesMissingBooth = this.OrderLine.Where(line => 
            line.ServiceTracking == "event_booth" && !line.EventBoothPendingIds.Any()).ToList();

        if (soLinesMissingBooth.Any())
        {
            var soLinesDescriptions = string.Join("", soLinesMissingBooth.Select(line => $"\n- {line.Name}"));
            throw new ValidationException($"Please make sure all your event-booth related lines are configured before confirming this order:{soLinesDescriptions}");
        }

        this.OrderLine.UpdateEventBooths();
        return res;
    }

    public ActionResult ActionViewBoothList()
    {
        var action = Env.Get<IrActionsActWindow>().ForXmlId("event_booth.event_booth_action");
        action.Domain = new[] { ("SaleOrderId", "in", new[] { this.Id }) };
        return action;
    }

    public List<(string, string, object)> GetProductCatalogDomain()
    {
        var domain = base.GetProductCatalogDomain();
        return domain.And(new[] { ("ServiceTracking", "!=", "event_booth") });
    }
}
