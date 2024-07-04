C#
public partial class PosConfig
{
    public void CheckSelfOrderOnlinePaymentMethodId()
    {
        if (this.SelfOrderingMode == "mobile" && this.SelfOrderingServiceMode == "each" && this.SelfOrderOnlinePaymentMethodId != null && !this.SelfOrderOnlinePaymentMethodId.GetOnlinePaymentProviders(this.Id, true).Any())
        {
            throw new Exception(Env.Translate("The online payment method used for self-order in a POS config must have at least one published payment provider supporting the currency of that POS config."));
        }
    }

    public Dictionary<string, object> GetSelfOrderingData()
    {
        var res = base.GetSelfOrderingData();
        var paymentMethods = GetSelfOrderingPaymentMethodsData(this.SelfOrderOnlinePaymentMethodId);
        res["pos_payment_methods"] += paymentMethods;
        return res;
    }
}
