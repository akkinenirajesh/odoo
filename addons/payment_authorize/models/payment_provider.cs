csharp
public partial class PaymentProvider
{
    public void ComputeFeatureSupportFields()
    {
        if (this.Code == "authorize")
        {
            this.SupportManualCapture = "full_only";
            this.SupportRefund = "full_only";
            this.SupportTokenization = true;
        }
        // Call the base method here
        // ...
    }

    public void ActionUpdateMerchantDetails()
    {
        if (this.State == "disabled")
        {
            throw new Exception("This action cannot be performed while the provider is disabled.");
        }

        AuthorizeAPI authorizeAPI = new AuthorizeAPI(this);

        // Validate the API Login ID and Transaction Key
        var resContent = authorizeAPI.TestAuthenticate();
        if (resContent.ContainsKey("err_msg"))
        {
            throw new Exception($"Failed to authenticate.\n{resContent["err_msg"]}");
        }

        // Update the merchant details
        resContent = authorizeAPI.MerchantDetails();
        if (resContent.ContainsKey("err_msg"))
        {
            throw new Exception($"Could not fetch merchant details:\n{resContent["err_msg"]}");
        }

        var currency = Env.Get<Core.Currency>().Search(resContent.Get("currencies"));
        this.AvailableCurrencyIds = currency;
        this.AuthorizeClientKey = resContent.Get("publicClientKey");
    }

    public decimal GetValidationAmount()
    {
        if (this.Code != "authorize")
        {
            // Call the base method here
            // ...
        }

        return 0.01M;
    }

    public Core.Currency GetValidationCurrency()
    {
        if (this.Code != "authorize")
        {
            // Call the base method here
            // ...
        }

        return this.AvailableCurrencyIds[0];
    }

    public string GetInlineFormValues()
    {
        var inlineFormValues = new
        {
            State = this.State,
            LoginId = this.AuthorizeLogin,
            ClientKey = this.AuthorizeClientKey
        };
        return JsonSerializer.Serialize(inlineFormValues);
    }

    public List<string> GetDefaultPaymentMethodCodes()
    {
        if (this.Code != "authorize")
        {
            // Call the base method here
            // ...
        }

        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }

    // Add other methods here ...
}
