csharp
public partial class PosOrder
{
    public bool UseSelfOrderOnlinePayment { get; set; }

    public Pos.PaymentMethod OnlinePaymentMethodId { get; set; }

    public void ComputeUseSelfOrderOnlinePayment()
    {
        this.UseSelfOrderOnlinePayment = Env.Get<Pos.PosConfig>().SelfOrderOnlinePaymentMethodId != null;
    }

    public void ComputeOnlinePaymentMethodId()
    {
        if (this.UseSelfOrderOnlinePayment)
        {
            this.OnlinePaymentMethodId = Env.Get<Pos.PosConfig>().SelfOrderOnlinePaymentMethodId;
        }
        else
        {
            // Call the base method for default online payment logic
        }
    }

    public List<PosOrder> Create(List<PosOrder> valsList)
    {
        foreach (var vals in valsList)
        {
            if (!vals.ContainsKey("UseSelfOrderOnlinePayment") || vals["UseSelfOrderOnlinePayment"] is bool && (bool)vals["UseSelfOrderOnlinePayment"])
            {
                var session = Env.Get<Pos.PosSession>().Browse(vals["SessionId"]);
                var config = session.ConfigId;
                vals["UseSelfOrderOnlinePayment"] = config.SelfOrderOnlinePaymentMethodId != null;
            }
        }
        return super.Create(valsList);
    }

    public bool Write(Dictionary<string, object> vals)
    {
        if (!vals.ContainsKey("UseSelfOrderOnlinePayment"))
        {
            return super.Write(vals);
        }

        var canChangeSelfOrderDomain = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "State", "draft" } } };
        if ((bool)vals["UseSelfOrderOnlinePayment"])
        {
            canChangeSelfOrderDomain = new List<Dictionary<string, object>> {
                new Dictionary<string, object> { { "State", "draft" } },
                new Dictionary<string, object> { { "ConfigId.SelfOrderOnlinePaymentMethodId", "!=" , null } }
            };
        }

        var canChangeSelfOrderOrders = this.FilteredDomain(canChangeSelfOrderDomain);
        var cannotChangeSelfOrderOrders = this - canChangeSelfOrderOrders;

        var res = true;
        if (canChangeSelfOrderOrders.Count > 0)
        {
            res = super(typeof(PosOrder), canChangeSelfOrderOrders).Write(vals) && res;
        }
        if (cannotChangeSelfOrderOrders.Count > 0)
        {
            var cleanVals = vals.ToDictionary(x => x.Key, x => x.Value);
            cleanVals.Remove("UseSelfOrderOnlinePayment");
            res = super(typeof(PosOrder), cannotChangeSelfOrderOrders).Write(cleanVals) && res;
        }

        return res;
    }

    public Dictionary<string, object> GetAndSetOnlinePaymentsData(decimal nextOnlinePaymentAmount)
    {
        var res = super.GetAndSetOnlinePaymentsData(nextOnlinePaymentAmount);
        if (!res.ContainsKey("paid_order") && !res.ContainsKey("deleted") && !nextOnlinePaymentAmount.Equals(0))
        {
            this.UseSelfOrderOnlinePayment = nextOnlinePaymentAmount == 0 && Env.Get<Pos.PosConfig>().SelfOrderOnlinePaymentMethodId != null;
        }
        return res;
    }
}
