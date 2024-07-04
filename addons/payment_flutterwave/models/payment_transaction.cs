csharp
public partial class PaymentTransaction 
{
    public void GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "flutterwave")
        {
            return;
        }

        string baseUrl = Env.Get<PaymentProvider>().GetBaseUrl(this.ProviderId);
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "tx_ref", this.Reference },
            { "amount", this.Amount },
            { "currency", this.CurrencyId.Name },
            { "redirect_url", baseUrl + FlutterwaveController.ReturnUrl },
            { "customer", new Dictionary<string, object>
                {
                    { "email", this.PartnerEmail },
                    { "name", this.PartnerName },
                    { "phonenumber", this.PartnerPhone },
                }
            },
            { "customizations", new Dictionary<string, object>
                {
                    { "title", this.CompanyId.Name },
                    { "logo", baseUrl + $"/web/image/res.company/{this.CompanyId.Id}/logo" },
                }
            },
            { "payment_options", const.PAYMENT_METHODS_MAPPING.GetValueOrDefault(
                this.PaymentMethodCode, this.PaymentMethodCode) },
        };
        Dictionary<string, object> paymentLinkData = Env.Get<PaymentProvider>().MakeRequest("payments", payload);

        processingValues["apiUrl"] = paymentLinkData["data"]["link"];
    }

    public void SendPaymentRequest()
    {
        if (this.ProviderCode != "flutterwave")
        {
            return;
        }

        if (this.TokenId == null)
        {
            throw new UserError("Flutterwave: The transaction is not linked to a token.");
        }

        string firstName = "";
        string lastName = "";
        payment_utils.SplitPartnerName(this.PartnerName, out firstName, out lastName);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "token", this.TokenId.ProviderRef },
            { "email", this.TokenId.FlutterwaveCustomerEmail },
            { "amount", this.Amount },
            { "currency", this.CurrencyId.Name },
            { "country", this.CompanyId.CountryId.Code },
            { "tx_ref", this.Reference },
            { "first_name", firstName },
            { "last_name", lastName },
            { "ip", payment_utils.GetCustomerIpAddress() },
        };

        Dictionary<string, object> responseContent = Env.Get<PaymentProvider>().MakeRequest("tokenized-charges", data);

        _logger.Info($"payment request response for transaction with reference {this.Reference}:\n{responseContent}");
        HandleNotificationData("flutterwave", responseContent["data"]);
    }

    public void GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "flutterwave")
        {
            return;
        }

        string reference = notificationData.GetValueOrDefault("tx_ref").ToString();
        if (reference == null)
        {
            throw new ValidationError("Flutterwave: Received data with missing reference.");
        }

        PaymentTransaction tx = Env.Get<PaymentTransaction>().Search(x => x.Reference == reference && x.ProviderCode == "flutterwave");
        if (tx == null)
        {
            throw new ValidationError($"Flutterwave: No transaction found matching reference {reference}.");
        }
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "flutterwave")
        {
            return;
        }

        Dictionary<string, object> verificationResponseContent = Env.Get<PaymentProvider>().MakeRequest("transactions/verify_by_reference", new Dictionary<string, object> { { "tx_ref", this.Reference } }, "GET");
        Dictionary<string, object> verifiedData = verificationResponseContent["data"];

        this.ProviderReference = verifiedData["id"].ToString();

        string paymentMethodType = verifiedData.GetValueOrDefault("payment_type").ToString() ?? "";
        if (paymentMethodType == "card")
        {
            paymentMethodType = verifiedData.GetValueOrDefault("card").ToString()?[0..3].ToLower();
        }
        PaymentMethod paymentMethod = Env.Get<PaymentMethod>().GetFromCode(paymentMethodType, const.PAYMENT_METHODS_MAPPING);
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;

        string paymentStatus = verifiedData["status"].ToString().ToLower();
        if (const.PAYMENT_STATUS_MAPPING["pending"].Contains(paymentStatus))
        {
            SetPending();
        }
        else if (const.PAYMENT_STATUS_MAPPING["done"].Contains(paymentStatus))
        {
            SetDone();
            if (this.Tokenize && verifiedData.GetValueOrDefault("card") != null && verifiedData["card"].ToString().Contains("token"))
            {
                TokenizeFromNotificationData(verifiedData);
            }
        }
        else if (const.PAYMENT_STATUS_MAPPING["cancel"].Contains(paymentStatus))
        {
            SetCancelled();
        }
        else if (const.PAYMENT_STATUS_MAPPING["error"].Contains(paymentStatus))
        {
            SetError($"An error occurred during the processing of your payment (status {paymentStatus}). Please try again.");
        }
        else
        {
            _logger.Warning($"Received data with invalid payment status ({paymentStatus}) for transaction with reference {this.Reference}.");
            SetError($"Flutterwave: Unknown payment status: {paymentStatus}");
        }
    }

    public void TokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        PaymentToken token = Env.Get<PaymentToken>().Create(new PaymentToken
        {
            ProviderId = this.ProviderId,
            PaymentMethodId = this.PaymentMethodId,
            PaymentDetails = notificationData["card"]["last_4digits"].ToString(),
            PartnerId = this.PartnerId,
            ProviderRef = notificationData["card"]["token"].ToString(),
            FlutterwaveCustomerEmail = notificationData["customer"]["email"].ToString(),
        });
        this.TokenId = token;
        this.Tokenize = false;
        _logger.Info($"created token with id {token.Id} for partner with id {this.PartnerId} from transaction with reference {this.Reference}");
    }

    private void SetPending()
    {
        this.State = "pending";
    }

    private void SetDone()
    {
        this.State = "done";
    }

    private void SetCancelled()
    {
        this.State = "cancel";
    }

    private void SetError(string message)
    {
        this.State = "error";
    }
}
