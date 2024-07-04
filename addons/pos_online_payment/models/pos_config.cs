csharp
public partial class PosConfig
{
    public void CheckOnlinePaymentMethods()
    {
        if (this.PaymentMethods.Any(pm => pm.IsOnlinePayment))
        {
            if (this.PaymentMethods.Count(pm => pm.IsOnlinePayment) > 1)
            {
                throw new Exception("A POS config cannot have more than one online payment method.");
            }
            var paymentMethodsWithOnlinePayment = this.PaymentMethods.Where(pm => pm.IsOnlinePayment).ToList();
            foreach (var paymentMethod in paymentMethodsWithOnlinePayment)
            {
                if (!paymentMethod.GetOnlinePaymentProviders(this.Id, true))
                {
                    throw new Exception("To use an online payment method in a POS config, it must have at least one published payment provider supporting the currency of that POS config.");
                }
            }
        }
    }

    public PosPaymentMethod GetCashierOnlinePaymentMethod()
    {
        return this.PaymentMethods.Where(pm => pm.IsOnlinePayment).FirstOrDefault();
    }
}
