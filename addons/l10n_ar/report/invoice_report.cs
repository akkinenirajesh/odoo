csharp
public partial class AccountInvoiceReport
{
    public override string ToString()
    {
        // Implement a meaningful string representation
        return $"Invoice Report {Date:d}";
    }

    protected override void OnCompute()
    {
        base.OnCompute();

        var move = Env.Get<Account.Move>(this.MoveId);
        var contactPartner = move.PartnerShippingId ?? move.PartnerId;

        this.L10nArStateId = contactPartner?.StateId;
        this.Date = move.Date;
    }

    protected override string BuildSelectQuery()
    {
        return $"{base.BuildSelectQuery()}, contact_partner.state_id as L10nArStateId, move.date";
    }

    protected override string BuildFromQuery()
    {
        return $"{base.BuildFromQuery()} LEFT JOIN res_partner contact_partner ON contact_partner.id = COALESCE(move.partner_shipping_id, move.partner_id)";
    }
}
