csharp
public partial class SaleOrder
{
    public override ActionResult Write(Dictionary<string, object> vals)
    {
        var result = base.Write(vals);

        if (OrderLine.Any(line => line.ServiceTracking == "event") && vals.ContainsKey("PartnerId"))
        {
            var registrationsToUpdate = Env.Get<Event.Registration>().Search(new[]
            {
                new object[] { "SaleOrderId", "in", new[] { Id } }
            });
            registrationsToUpdate.Write(new Dictionary<string, object> { { "PartnerId", vals["PartnerId"] } });
        }

        return result;
    }

    public ActionResult ActionConfirm()
    {
        var unconfirmedRegistrations = OrderLine.SelectMany(line => line.RegistrationIds)
            .Where(reg => new[] { "draft", "cancel" }.Contains(reg.State));

        var res = base.ActionConfirm();

        unconfirmedRegistrations.UpdateMailSchedulers();

        if (OrderLine.Any(line => line.ServiceTracking == "event"))
        {
            var soLinesMissingEvents = OrderLine.Where(line => line.ServiceTracking == "event" && line.EventId == null).ToList();
            if (soLinesMissingEvents.Any())
            {
                var soLinesDescriptions = string.Join("", soLinesMissingEvents.Select(line => $"\n- {line.Name}"));
                throw new ValidationException($"Please make sure all your event related lines are configured before confirming this order:{soLinesDescriptions}");
            }

            OrderLine.InitRegistrations();

            return Env.Get<Core.IrActionsActWindow>().ForXmlId("event_sale.action_sale_order_event_registration", new { DefaultSaleOrderId = Id });
        }

        return res;
    }

    public ActionResult ActionViewAttendeeList()
    {
        var action = Env.Get<Core.IrActionsActions>().ForXmlId("event.event_registration_action_tree");
        action.Domain = new object[] { new object[] { "SaleOrderId", "in", new[] { Id } } };
        return action;
    }

    private void _ComputeAttendeeCount()
    {
        var saleOrdersData = Env.Get<Event.Registration>().ReadGroup(
            new object[]
            {
                new object[] { "SaleOrderId", "in", new[] { Id } },
                new object[] { "State", "!=", "cancel" }
            },
            new[] { "SaleOrderId" },
            new[] { "__count" }
        );

        var attendeeCountData = saleOrdersData.ToDictionary(
            item => (int)item["SaleOrderId"],
            item => (int)item["__count"]
        );

        AttendeeCount = attendeeCountData.TryGetValue(Id, out var count) ? count : 0;
    }

    public override List<object> GetProductCatalogDomain()
    {
        var domain = base.GetProductCatalogDomain();
        return domain.Concat(new object[] { new object[] { "ServiceTracking", "!=", "event" } }).ToList();
    }
}
