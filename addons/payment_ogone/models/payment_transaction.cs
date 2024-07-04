csharp
public partial class PaymentTransaction
{
    public virtual string ComputeReference(string providerCode, string prefix, string separator)
    {
        if (providerCode != "ogone")
        {
            return Env.Call<string>("Payment.PaymentTransaction", "_computeReference", providerCode, prefix, separator);
        }
        if (string.IsNullOrEmpty(prefix))
        {
            prefix = ComputeReferencePrefix(providerCode, separator);
        }
        prefix = Env.Call<string>("Payment.PaymentTransaction", "SingularizeReferencePrefix", prefix, 40);
        return Env.Call<string>("Payment.PaymentTransaction", "_computeReference", providerCode, prefix, separator);
    }

    public virtual string ComputeReferencePrefix(string providerCode, string separator)
    {
        return Env.Call<string>("Payment.PaymentTransaction", "_computeReferencePrefix", providerCode, separator);
    }

    public virtual Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        var res = Env.Call<Dictionary<string, object>>("Payment.PaymentTransaction", "_getSpecificRenderingValues", processingValues);
        if (ProviderCode != "ogone")
        {
            return res;
        }

        var returnUrl = Env.Call<string>("Payment.PaymentTransaction", "UrlJoin", Provider.GetBaseUrl(), "/ogone/return");
        var renderingValues = new Dictionary<string, object>
        {
            { "PSPID", Provider.OgonePspid },
            { "ORDERID", Reference },
            { "AMOUNT", Env.Call<decimal>("Payment.PaymentTransaction", "ToMinorCurrencyUnits", Amount, null, 2) },
            { "CURRENCY", Currency.Name },
            { "LANGUAGE", PartnerLang ?? "en_US" },
            { "EMAIL", PartnerEmail ?? "" },
            { "CN", PartnerName ?? "" },
            { "OWNERADDRESS", PartnerAddress ?? "" },
            { "OWNERZIP", PartnerZip ?? "" },
            { "OWNERTOWN", PartnerCity ?? "" },
            { "OWNERCTY", PartnerCountry.Code ?? "" },
            { "OWNERTELNO", PartnerPhone ?? "" },
            { "OPERATION", "SAL" },
            { "USERID", Provider.OgoneUserid },
            { "ACCEPTURL", returnUrl },
            { "DECLINEURL", returnUrl },
            { "EXCEPTIONURL", returnUrl },
            { "CANCELURL", returnUrl },
            { "PM", Env.Call<string>("Payment.PaymentTransaction", "GetPaymentMethodMapping", PaymentMethodCode, PaymentMethodCode) },
        };
        if (Tokenize)
        {
            renderingValues.Add("ALIAS", $"ODOO-ALIAS-{Guid.NewGuid().ToString("N")}");
            renderingValues.Add("ALIASUSAGE", "Storing your payment details is necessary for future use.");
        }
        renderingValues.Add("SHASIGN", Provider.OgoneGenerateSignature(renderingValues, false).ToUpper());
        renderingValues.Add("api_url", Provider.OgoneGetApiUrl("hosted_payment_page"));
        return renderingValues;
    }

    public virtual void SendPaymentRequest()
    {
        Env.Call("Payment.PaymentTransaction", "_sendPaymentRequest", this);
        if (ProviderCode != "ogone")
        {
            return;
        }

        if (Token == null)
        {
            throw new Exception("Ogone: The transaction is not linked to a token.");
        }

        var data = new Dictionary<string, object>
        {
            { "PSPID", Provider.OgonePspid },
            { "ORDERID", Reference },
            { "USERID", Provider.OgoneUserid },
            { "PSWD", Provider.OgonePassword },
            { "AMOUNT", Env.Call<decimal>("Payment.PaymentTransaction", "ToMinorCurrencyUnits", Amount, null, 2) },
            { "CURRENCY", Currency.Name },
            { "CN", PartnerName ?? "" },
            { "EMAIL", PartnerEmail ?? "" },
            { "OWNERADDRESS", PartnerAddress ?? "" },
            { "OWNERZIP", PartnerZip ?? "" },
            { "OWNERTOWN", PartnerCity ?? "" },
            { "OWNERCTY", PartnerCountry.Code ?? "" },
            { "OWNERTELNO", PartnerPhone ?? "" },
            { "OPERATION", "SAL" },
            { "ALIAS", Token.ProviderRef },
            { "ALIASPERSISTEDAFTERUSE", "Y" },
            { "ECI", 9 },
        };
        data.Add("SHASIGN", Provider.OgoneGenerateSignature(data, false));
        Env.Log(LogLevel.Info, $"payment request response for transaction with reference {Reference}:\n{data.Where(kvp => kvp.Key != "PSWD").Select(kvp => $"{kvp.Key}: {kvp.Value}").Aggregate((a, b) => $"{a}\n{b}")}");
        var responseContent = Provider.OgoneMakeRequest(data);
        try
        {
            var tree = Env.Call<object>("Payment.PaymentTransaction", "Fromstring", responseContent);
            Env.Log(LogLevel.Info, $"payment request response (as an etree) for transaction with reference {Reference}:\n{tree}");
        }
        catch (Exception ex)
        {
            throw new Exception("Ogone: Received badly structured response from the API.", ex);
        }

        var feedbackData = new Dictionary<string, object>
        {
            { "ORDERID", Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", tree, "orderID") },
            { "tree", tree },
        };
        Env.Log(LogLevel.Info, $"handling feedback data from Ogone for transaction with reference {Reference} with data:\n{feedbackData}");
        HandleNotificationData("ogone", feedbackData);
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        var tx = Env.Call<PaymentTransaction>("Payment.PaymentTransaction", "_getTxFromNotificationData", providerCode, notificationData);
        if (providerCode != "ogone" || tx != null)
        {
            return tx;
        }

        var reference = Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "ORDERID");
        tx = Env.Call<PaymentTransaction>("Payment.PaymentTransaction", "Search", new[] { new Tuple<string, object>("Reference", reference), new Tuple<string, object>("ProviderCode", "ogone") });
        if (tx == null)
        {
            throw new Exception($"Ogone: No transaction found matching reference {reference}.");
        }
        return tx;
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        Env.Call("Payment.PaymentTransaction", "_processNotificationData", this, notificationData);
        if (ProviderCode != "ogone")
        {
            return;
        }

        if (notificationData.ContainsKey("tree"))
        {
            notificationData = (Dictionary<string, object>)notificationData["tree"];
        }

        ProviderReference = Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "PAYID");

        var paymentMethodCode = Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "BRAND");
        var paymentMethod = Env.Call<PaymentMethod>("Payment.PaymentMethod", "_get_from_code", paymentMethodCode, Env.Call<Dictionary<string, string>>("Payment.PaymentTransaction", "GetPaymentMethodMapping"));
        PaymentMethod = paymentMethod ?? PaymentMethod;

        var paymentStatus = int.Parse(Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "STATUS") ?? "0");
        if (Env.Call<List<string>>("Payment.PaymentTransaction", "GetPaymentStatusMapping", "pending").Contains(paymentStatus))
        {
            SetPending();
        }
        else if (Env.Call<List<string>>("Payment.PaymentTransaction", "GetPaymentStatusMapping", "done").Contains(paymentStatus))
        {
            var hasTokenData = notificationData.ContainsKey("ALIAS");
            if (Tokenize && hasTokenData)
            {
                OgoneTokenizeFromNotificationData(notificationData);
            }
            SetDone();
        }
        else if (Env.Call<List<string>>("Payment.PaymentTransaction", "GetPaymentStatusMapping", "cancel").Contains(paymentStatus))
        {
            SetCanceled();
        }
        else if (Env.Call<List<string>>("Payment.PaymentTransaction", "GetPaymentStatusMapping", "declined").Contains(paymentStatus))
        {
            var reason = notificationData.ContainsKey("NCERRORPLUS") ? Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "NCERRORPLUS") : notificationData.ContainsKey("NCERROR") ? $"Error code: {Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "NCERROR")}" : "Unknown reason";
            Env.Log(LogLevel.Info, $"the payment has been declined: {reason}.");
            SetError($"Ogone: The payment has been declined: {reason}");
        }
        else
        {
            Env.Log(LogLevel.Info, $"received data with invalid payment status ({paymentStatus}) for transaction with reference {Reference}");
            SetError($"Ogone: Received data with invalid payment status: {paymentStatus}");
        }
    }

    public virtual void OgoneTokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        var token = Env.Call<PaymentToken>("Payment.PaymentToken", "Create", new Dictionary<string, object>
        {
            { "Provider", Provider.Id },
            { "PaymentMethod", PaymentMethod.Id },
            { "PaymentDetails", Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "CARDNO").Substring(Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "CARDNO").Length - 4) },
            { "Partner", Partner.Id },
            { "ProviderRef", Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", notificationData, "ALIAS") },
        });
        Env.Call("Payment.PaymentTransaction", "Write", this, new Dictionary<string, object>
        {
            { "Token", token.Id },
            { "Tokenize", false },
        });
        Env.Log(LogLevel.Info, $"created token with id {token.Id} for partner with id {Partner.Id} from transaction with reference {Reference}");
    }

    public virtual void SetPending()
    {
        Env.Call("Payment.PaymentTransaction", "_set_pending", this);
    }

    public virtual void SetDone()
    {
        Env.Call("Payment.PaymentTransaction", "_set_done", this);
    }

    public virtual void SetCanceled()
    {
        Env.Call("Payment.PaymentTransaction", "_set_canceled", this);
    }

    public virtual void SetError(string message)
    {
        Env.Call("Payment.PaymentTransaction", "_set_error", this, message);
    }

    public virtual string SingularizeReferencePrefix(string prefix, int maxLength)
    {
        return Env.Call<string>("Payment.PaymentTransaction", "SingularizeReferencePrefix", prefix, maxLength);
    }

    public virtual string GetBaseUrl()
    {
        return Env.Call<string>("Payment.PaymentTransaction", "GetBaseUrl");
    }

    public virtual string UrlJoin(string baseurl, string url)
    {
        return Env.Call<string>("Payment.PaymentTransaction", "UrlJoin", baseurl, url);
    }

    public virtual decimal ToMinorCurrencyUnits(decimal amount, object currency, int digits)
    {
        return Env.Call<decimal>("Payment.PaymentTransaction", "ToMinorCurrencyUnits", amount, currency, digits);
    }

    public virtual string GetPaymentMethodMapping(string code, string defaultCode)
    {
        return Env.Call<string>("Payment.PaymentTransaction", "GetPaymentMethodMapping", code, defaultCode);
    }

    public virtual string GetAttributeValue(object tree, string attributeName)
    {
        return Env.Call<string>("Payment.PaymentTransaction", "GetAttributeValue", tree, attributeName);
    }

    public virtual object Fromstring(string content)
    {
        return Env.Call<object>("Payment.PaymentTransaction", "Fromstring", content);
    }

    public virtual PaymentTransaction Search(params Tuple<string, object>[] criteria)
    {
        return Env.Call<PaymentTransaction>("Payment.PaymentTransaction", "Search", criteria);
    }

    public virtual void Write(Dictionary<string, object> values)
    {
        Env.Call("Payment.PaymentTransaction", "Write", this, values);
    }

    public virtual void Create(Dictionary<string, object> values)
    {
        Env.Call("Payment.PaymentTransaction", "Create", values);
    }

    public virtual void Log(LogLevel level, string message)
    {
        Env.Log(level, message);
    }

    public virtual void HandleNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        Env.Call("Payment.PaymentTransaction", "_handle_notification_data", this, providerCode, notificationData);
    }

    public virtual List<string> GetPaymentStatusMapping(string status)
    {
        return Env.Call<List<string>>("Payment.PaymentTransaction", "GetPaymentStatusMapping", status);
    }
}
