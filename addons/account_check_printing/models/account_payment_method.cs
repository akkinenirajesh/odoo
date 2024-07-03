csharp
public partial class AccountPaymentMethod
{
    public IDictionary<string, object> GetPaymentMethodInformation()
    {
        var res = base.GetPaymentMethodInformation();
        res["check_printing"] = new Dictionary<string, object>
        {
            { "mode", "multi" },
            { "domain", new List<object> { new List<object> { "type", "=", "bank" } } }
        };
        return res;
    }
}
