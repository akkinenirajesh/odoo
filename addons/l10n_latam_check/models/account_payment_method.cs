csharp
public partial class AccountPaymentMethod
{
    public Dictionary<string, PaymentMethodInfo> GetPaymentMethodInformation()
    {
        var res = base.GetPaymentMethodInformation();
        res["new_third_party_checks"] = new PaymentMethodInfo { Mode = "multi", Domain = new List<object> { new List<object> { "Type", "=", "cash" } } };
        res["in_third_party_checks"] = new PaymentMethodInfo { Mode = "multi", Domain = new List<object> { new List<object> { "Type", "=", "cash" } } };
        res["out_third_party_checks"] = new PaymentMethodInfo { Mode = "multi", Domain = new List<object> { new List<object> { "Type", "=", "cash" } } };
        return res;
    }
}

public class PaymentMethodInfo
{
    public string Mode { get; set; }
    public List<object> Domain { get; set; }
}
