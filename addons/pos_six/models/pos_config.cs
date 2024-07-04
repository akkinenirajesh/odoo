csharp
public partial class PosConfig 
{
    public bool ForceHttp()
    {
        bool enforceHttps = Env.GetParam("point_of_sale.enforce_https");
        if (!enforceHttps && this.PaymentMethodIds.Any(pm => pm.UsePaymentTerminal == "six"))
        {
            return true;
        }
        return false;
    }
}
