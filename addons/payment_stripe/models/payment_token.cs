C#
public partial class PaymentToken 
{
    public void StripeSCAMigrateCustomer()
    {
        // Fetch the available payment method of type 'card' for the given customer
        var responseContent = Env.Provider.StripeMakeRequest("payment_methods", new { customer = this.ProviderRef, type = "card", limit = 1 }, "GET");
        Env.Logger.Info("received payment_methods response:\n{0}", responseContent);

        // Store the payment method ID on the token
        var paymentMethods = responseContent.Get("data") as List<dynamic>;
        var paymentMethodId = paymentMethods?.FirstOrDefault()?.Get("id");
        if (paymentMethodId == null)
        {
            throw new ValidationError("Stripe: Unable to convert payment token to new API.");
        }
        this.StripePaymentMethod = paymentMethodId;
        Env.Logger.Info("converted token with id {0} to new API", this.Id);
    }
}
