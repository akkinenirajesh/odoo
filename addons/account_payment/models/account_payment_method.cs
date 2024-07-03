csharp
public partial class AccountPaymentMethod
{
    public Dictionary<string, Dictionary<string, object>> GetPaymentMethodInformation()
    {
        var res = base.GetPaymentMethodInformation();

        var paymentProvider = Env.Get<PaymentProvider>();
        var codeSelection = paymentProvider.Fields["Code"].Selection;

        foreach (var (code, _) in codeSelection)
        {
            if (code == "none" || code == "custom")
            {
                continue;
            }

            res[code] = new Dictionary<string, object>
            {
                ["Mode"] = "electronic",
                ["Domain"] = new List<object> { new List<object> { "Type", "=", "bank" } }
            };
        }

        return res;
    }
}
