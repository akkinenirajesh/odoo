csharp
public partial class WebsiteSalePaymentToken 
{
    public WebsiteSalePaymentToken(Buvi.Environment env)
    {
        this.Env = env;
    }

    public Buvi.Environment Env { get; set; }

    public virtual Buvi.Recordset<PaymentToken> GetAvailableTokens(bool isExpressCheckout)
    {
        if (isExpressCheckout)
        {
            return Env.Ref("Payment.PaymentToken").Records;
        }

        return Env.Ref("Payment.PaymentToken").CallMethod("_get_available_tokens");
    }
}
