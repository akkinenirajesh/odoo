csharp
public partial class PosPaymentMethod
{
    public virtual bool IsOnlinePayment { get; set; }
    public virtual string Name { get; set; }
    public virtual AccountJournal JournalId { get; set; }
    public virtual AccountAccount PaymentAccountId { get; set; }
    public virtual bool CashControl { get; set; }
    public virtual string InvoiceMethod { get; set; }
    public virtual string Barcode { get; set; }
    public virtual bool Default { get; set; }
    public virtual bool Active { get; set; }
    public virtual int Sequence { get; set; }
    public virtual string Code { get; set; }
    public virtual bool PartnerTip { get; set; }
    
    public virtual object LoadPosSelfDataDomain(object data)
    {
        if ((string)data["pos.config"]["data"][0]["self_ordering_mode"] == "kiosk")
        {
            var domain = (object)Env.Call("PosPaymentMethod", "_load_pos_self_data_domain", data);
            domain = Env.Call("expression", "OR", new object[] { new object[] { new object[] { "IsOnlinePayment", "=", true } }, domain });
            return domain;
        }
        else
        {
            return new object[] { new object[] { "IsOnlinePayment", "=", true } };
        }
    }
    public virtual object _load_pos_self_data_domain(object data)
    {
        return Env.Call("PosPaymentMethod", "LoadPosSelfDataDomain", data);
    }

    public virtual object _load_pos_self_data_domain(object data)
    {
        return Env.Call("PosPaymentMethod", "LoadPosSelfDataDomain", data);
    }
}
