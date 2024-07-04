csharp
public partial class PaymentMethod 
{
    public void ComputeIsPrimary()
    {
        this.IsPrimary = this.PrimaryPaymentMethod == null;
    }

    public void SearchIsPrimary(string operator, bool value)
    {
        if (operator == "=" && value)
        {
            // Return a query that filters for payment methods without a primary payment method
            Env.Search<PaymentMethod>(x => x.PrimaryPaymentMethod == null);
        }
        else if (operator == "=" && !value)
        {
            // Return a query that filters for payment methods with a primary payment method
            Env.Search<PaymentMethod>(x => x.PrimaryPaymentMethod != null);
        }
        else
        {
            throw new NotImplementedException("Operation not supported.");
        }
    }

    public void OnChangeWarnBeforeDisablingTokens()
    {
        // Logic for warning before disabling tokens
    }

    public void OnChangeProviderIdsWarnBeforeAttachingPaymentMethod()
    {
        // Logic for warning before attaching payment method to a provider
    }

    public void Write(Dictionary<string, object> values)
    {
        // Handle archiving, detaching providers, blocking tokenization
        // Prevent enabling a payment method if not linked to an enabled provider
        // Call base write method
    }

    public PaymentMethod[] GetCompatiblePaymentMethods(
        int[] providerIds,
        int partnerId,
        int? currencyId = null,
        bool forceTokenization = false,
        bool isExpressCheckout = false,
        Dictionary<string, object> report = null,
        Dictionary<string, object> kwargs = null)
    {
        // Search and return compatible payment methods
    }

    public PaymentMethod GetFromCode(string code, Dictionary<string, string> mapping = null)
    {
        // Get the payment method corresponding to the given provider-specific code
    }
}
