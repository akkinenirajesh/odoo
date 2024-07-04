csharp
public partial class PaymentToken
{
    public string BuildDisplayName(object[] args, bool shouldPad, object kwargs)
    {
        if (this.ProviderCode != "demo")
        {
            return Env.Call("Payment.PaymentToken", "_build_display_name", args, shouldPad, kwargs);
        }
        return Env.Call("Payment.PaymentToken", "_build_display_name", args, false, kwargs);
    }
}
