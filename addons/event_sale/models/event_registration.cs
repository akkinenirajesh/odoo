csharp
public partial class EventRegistration
{
    public void ComputeRegistrationStatus()
    {
        var saleOrder = this.SaleOrderId;
        var saleOrderLine = this.SaleOrderLineId;

        if (saleOrder.State == "cancel")
        {
            this.State = EventRegistrationState.Cancel;
        }

        if (saleOrderLine == null || Env.FloatIsZero(saleOrderLine.PriceTotal, saleOrderLine.CurrencyId.Rounding))
        {
            this.SaleStatus = EventRegistrationSaleStatus.Free;
            if (string.IsNullOrEmpty(this.State.ToString()) || this.State == EventRegistrationState.Draft)
            {
                this.State = EventRegistrationState.Open;
            }
        }
        else
        {
            if (saleOrder.State == "sale" && this.State != EventRegistrationState.Cancel)
            {
                this.SaleStatus = EventRegistrationSaleStatus.Sold;
                if (string.IsNullOrEmpty(this.State.ToString()) || this.State == EventRegistrationState.Draft || this.State == EventRegistrationState.Cancel)
                {
                    this.State = EventRegistrationState.Open;
                }
            }
            else
            {
                this.SaleStatus = EventRegistrationSaleStatus.ToPay;
                if (this.State != EventRegistrationState.Cancel)
                {
                    this.State = EventRegistrationState.Draft;
                }
            }
        }
    }

    public void ComputeUtmCampaignId()
    {
        if (this.SaleOrderId?.CampaignId != null)
        {
            this.UtmCampaignId = this.SaleOrderId.CampaignId;
        }
        else if (this.UtmCampaignId == null)
        {
            this.UtmCampaignId = null;
        }
    }

    public void ComputeUtmSourceId()
    {
        if (this.SaleOrderId?.SourceId != null)
        {
            this.UtmSourceId = this.SaleOrderId.SourceId;
        }
        else if (this.UtmSourceId == null)
        {
            this.UtmSourceId = null;
        }
    }

    public void ComputeUtmMediumId()
    {
        if (this.SaleOrderId?.MediumId != null)
        {
            this.UtmMediumId = this.SaleOrderId.MediumId;
        }
        else if (this.UtmMediumId == null)
        {
            this.UtmMediumId = null;
        }
    }

    public object ActionViewSaleOrder()
    {
        var action = Env.Ref("sale.action_orders");
        action.Views = new List<object> { new object[] { false, "form" } };
        action.ResId = this.SaleOrderId.Id;
        return action;
    }

    public void SynchronizeSoLineValues(Sale.SaleOrderLine soLine)
    {
        if (soLine != null)
        {
            this.PartnerId = Env.User.IsPublic() && Env.User.PartnerId == soLine.OrderId.PartnerId ? null : soLine.OrderId.PartnerId;
            this.EventId = soLine.EventId;
            this.EventTicketId = soLine.EventTicketId;
            this.SaleOrderId = soLine.OrderId;
            this.SaleOrderLineId = soLine;
        }
    }

    public void SaleOrderTicketTypeChangeNotify(Event.EventEventTicket newEventTicket)
    {
        var fallbackUserId = Env.User.IsPublic() ? Env.Ref("base.user_admin").Id : Env.User.Id;
        var userId = this.EventId.UserId?.Id ?? this.SaleOrderId.UserId?.Id ?? fallbackUserId;

        var renderContext = new Dictionary<string, object>
        {
            { "registration", this },
            { "old_ticket_name", this.EventTicketId.Name },
            { "new_ticket_name", newEventTicket.Name }
        };

        this.SaleOrderId.ActivityScheduleWithView(
            "mail.mail_activity_data_warning",
            userId,
            "event_sale.event_ticket_id_change_exception",
            renderContext);
    }

    public Dictionary<string, object> GetRegistrationSummary()
    {
        var summary = base.GetRegistrationSummary();
        summary["sale_status"] = this.SaleStatus;
        summary["sale_status_value"] = Env.GetSelectionValue(typeof(EventRegistrationSaleStatus), this.SaleStatus);
        summary["has_to_pay"] = this.SaleStatus == EventRegistrationSaleStatus.ToPay;
        return summary;
    }
}
