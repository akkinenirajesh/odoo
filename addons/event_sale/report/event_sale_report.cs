csharp
public partial class EventSaleReport
{
    public override string ToString()
    {
        return $"Event Sale Report: {EventRegistrationName} - {Event?.Name}";
    }

    public void Init()
    {
        Env.Cr.Execute("DROP VIEW IF EXISTS Event_EventSaleReport");
        Env.Cr.Execute($"CREATE OR REPLACE VIEW Event_EventSaleReport AS ({Query()})");
    }

    private string Query(List<string> with = null, List<string> select = null, List<string> join = null, List<string> groupBy = null)
    {
        return string.Join("\n", new[]
        {
            WithClause(with),
            SelectClause(select),
            FromClause(join),
            GroupByClause(groupBy)
        }.Where(clause => !string.IsNullOrEmpty(clause)));
    }

    private string WithClause(List<string> with)
    {
        return with != null && with.Any()
            ? "WITH\n    " + string.Join(",\n    ", with)
            : string.Empty;
    }

    private string SelectClause(List<string> select)
    {
        var baseSelect = @"
SELECT
    ROW_NUMBER() OVER (ORDER BY EventRegistration.Id) AS Id,

    EventRegistration.Id AS EventRegistrationId,
    EventRegistration.CompanyId AS CompanyId,
    EventRegistration.EventId AS EventId,
    EventRegistration.EventTicketId AS EventTicketId,
    EventRegistration.CreateDate AS EventRegistrationCreateDate,
    EventRegistration.Name AS EventRegistrationName,
    EventRegistration.State AS EventRegistrationState,
    EventRegistration.Active AS Active,
    EventRegistration.SaleOrderId AS SaleOrderId,
    EventRegistration.SaleOrderLineId AS SaleOrderLineId,
    EventRegistration.SaleStatus AS SaleStatus,

    EventEvent.EventTypeId AS EventTypeId,
    EventEvent.DateBegin AS EventDateBegin,
    EventEvent.DateEnd AS EventDateEnd,

    EventEventTicket.Price AS EventTicketPrice,

    SaleOrder.DateOrder AS SaleOrderDate,
    SaleOrder.PartnerInvoiceId AS InvoicePartnerId,
    SaleOrder.PartnerId AS SaleOrderPartnerId,
    SaleOrder.State AS SaleOrderState,
    SaleOrder.UserId AS SaleOrderUserId,

    SaleOrderLine.ProductId AS ProductId,
    CASE
        WHEN SaleOrderLine.ProductUomQty = 0 THEN 0
        ELSE
        SaleOrderLine.PriceTotal
            / CASE COALESCE(SaleOrder.CurrencyRate, 0) WHEN 0 THEN 1.0 ELSE SaleOrder.CurrencyRate END
            / SaleOrderLine.ProductUomQty
    END AS SalePrice,
    CASE
        WHEN SaleOrderLine.ProductUomQty = 0 THEN 0
        ELSE
        SaleOrderLine.PriceSubtotal
            / CASE COALESCE(SaleOrder.CurrencyRate, 0) WHEN 0 THEN 1.0 ELSE SaleOrder.CurrencyRate END
            / SaleOrderLine.ProductUomQty
    END AS SalePriceUntaxed";

        return baseSelect + (select != null && select.Any() ? ",\n    " + string.Join(",\n    ", select) : string.Empty);
    }

    private string FromClause(List<string> join)
    {
        var baseFrom = @"
FROM EventRegistration
LEFT JOIN EventEvent ON EventEvent.Id = EventRegistration.EventId
LEFT JOIN EventEventTicket ON EventEventTicket.Id = EventRegistration.EventTicketId
LEFT JOIN SaleOrder ON SaleOrder.Id = EventRegistration.SaleOrderId
LEFT JOIN SaleOrderLine ON SaleOrderLine.Id = EventRegistration.SaleOrderLineId
";

        return baseFrom + (join != null ? string.Join("\n", join) + "\n" : string.Empty);
    }

    private string GroupByClause(List<string> groupBy)
    {
        return groupBy != null && groupBy.Any()
            ? "GROUP BY\n    " + string.Join(",\n    ", groupBy)
            : string.Empty;
    }
}
