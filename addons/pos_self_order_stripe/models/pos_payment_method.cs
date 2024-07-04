csharp
public partial class PosPaymentMethod
{
    public virtual object StripePaymentIntent(decimal amountTotal)
    {
        // Implement Stripe Payment Intent logic
        return null;
    }

    public virtual object PaymentRequestFromKiosk(object order)
    {
        if (this.UsePaymentTerminal != "stripe")
        {
            return Env.Call("super", "PaymentRequestFromKiosk", order);
        }
        else
        {
            return this.StripePaymentIntent(order.As<object>().GetValue<decimal>("amount_total"));
        }
    }
}
