csharp
public partial class SaleReport
{
    public SaleReport(BuviContext env)
    {
        this.Env = env;
    }

    public BuviContext Env { get; private set; }

    public virtual void SelectAdditionalFields()
    {
        var res = Env.CallMethod<Dictionary<string, object>>("Sale.Report", "_select_additional_fields");
        res["WebsiteId"] = "s.website_id";
        res["IsAbandonedCart"] = @"
            s.date_order <= (timezone('utc', now()) - ((COALESCE(w.cart_abandoned_delay, '1.0') || ' hour')::INTERVAL))
            AND s.website_id IS NOT NULL
            AND s.state = 'draft'
            AND s.partner_id != %s" % this.Env.Ref("base.public_partner").Id;
    }

    public virtual void FromSale()
    {
        var res = Env.CallMethod<string>("Sale.Report", "_from_sale");
        res += @"
            LEFT JOIN website w ON w.id = s.website_id";
    }

    public virtual void GroupBySale()
    {
        var res = Env.CallMethod<string>("Sale.Report", "_group_by_sale");
        res += @",
            s.website_id,
            w.cart_abandoned_delay";
    }
}
