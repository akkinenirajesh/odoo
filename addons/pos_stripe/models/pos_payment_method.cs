csharp
public partial class PosPaymentMethod
{
    public string StripeSerialNumber { get; set; }

    public virtual List<PosPaymentMethod> GetPaymentTerminalSelection()
    {
        var result = Env.CallMethod<List<PosPaymentMethod>>(this, "GetPaymentTerminalSelection", new List<object>());
        result.AddRange(new List<PosPaymentMethod>() { new PosPaymentMethod() { Name = "Stripe", Code = "stripe" } });
        return result;
    }

    public virtual List<string> LoadPosDataFields(long configId)
    {
        var result = Env.CallMethod<List<string>>(this, "LoadPosDataFields", new List<object>() { configId });
        result.Add("StripeSerialNumber");
        return result;
    }

    public virtual void CheckStripeSerialNumber()
    {
        if (string.IsNullOrEmpty(StripeSerialNumber))
        {
            return;
        }
        var existingPaymentMethod = Env.Search<PosPaymentMethod>(new List<object>() { "StripeSerialNumber", "=", StripeSerialNumber }, new List<object>() { "Id", "!=", Id }, 1);
        if (existingPaymentMethod != null)
        {
            throw new Exception($"Terminal {StripeSerialNumber} is already used on payment method {existingPaymentMethod.DisplayName}.");
        }
    }

    public virtual PaymentProvider GetStripePaymentProvider()
    {
        var stripePaymentProvider = Env.Search<PaymentProvider>(new List<object>() { "Code", "=", "stripe", "CompanyId", "=", Env.Company.Id }, 1);
        if (stripePaymentProvider == null)
        {
            throw new Exception($"Stripe payment provider for company {Env.Company.Name} is missing");
        }
        return stripePaymentProvider;
    }

    public virtual string GetStripeSecretKey()
    {
        var stripeSecretKey = GetStripePaymentProvider().StripeSecretKey;
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            throw new Exception($"Complete the Stripe onboarding for company {Env.Company.Name}.");
        }
        return stripeSecretKey;
    }

    public virtual dynamic StripeConnectionToken()
    {
        if (!Env.User.IsInGroup("point_of_sale.group_pos_user"))
        {
            throw new Exception("Do not have access to fetch token from Stripe");
        }
        var endpoint = "https://api.stripe.com/v1/terminal/connection_tokens";
        try
        {
            var resp = Env.CallMethod<dynamic>(this, "RequestPost", new List<object>() { endpoint, GetStripeSecretKey(), "" });
            return resp;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to call stripe_connection_token endpoint\n{ex.Message}");
        }
    }

    public virtual decimal StripeCalculateAmount(decimal amount)
    {
        var currency = Journal.Currency ?? Company.Currency;
        return Math.Round(amount / currency.Rounding);
    }

    public virtual dynamic StripePaymentIntent(decimal amount)
    {
        if (!Env.User.IsInGroup("point_of_sale.group_pos_user"))
        {
            throw new Exception("Do not have access to fetch token from Stripe");
        }
        var endpoint = "https://api.stripe.com/v1/payment_intents";
        var currency = Journal.Currency ?? Company.Currency;
        var paramsList = new List<object>() {
            ("currency", currency.Name),
            ("amount", StripeCalculateAmount(amount)),
            ("payment_method_types[]", "card_present"),
            ("capture_method", "manual")
        };
        if (currency.Name == "AUD" && Company.CountryCode == "AU")
        {
            paramsList.Add(("payment_method_options[card_present][capture_method]", "manual_preferred"));
        }
        else if (currency.Name == "CAD" && Company.CountryCode == "CA")
        {
            paramsList.Add(("payment_method_types[]", "interac_present"));
        }
        try
        {
            var resp = Env.CallMethod<dynamic>(this, "RequestPost", new List<object>() { endpoint, GetStripeSecretKey(), "", paramsList });
            return resp;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to call stripe_payment_intent endpoint\n{ex.Message}");
        }
    }

    public virtual dynamic StripeCapturePayment(string paymentIntentId, decimal? amount = null)
    {
        if (!Env.User.IsInGroup("point_of_sale.group_pos_user"))
        {
            throw new Exception("Do not have access to fetch token from Stripe");
        }
        var endpoint = $"payment_intents/{paymentIntentId}/capture";
        var data = new Dictionary<string, object>();
        if (amount != null)
        {
            data.Add("amount_to_capture", StripeCalculateAmount(amount.Value));
        }
        return GetStripePaymentProvider().StripeMakeRequest(endpoint, data);
    }

    public virtual dynamic ActionStripeKey()
    {
        return Env.ActionWindow(new List<object>() { "PaymentProvider", "form", GetStripePaymentProvider().Id, "Stripe" });
    }
}
