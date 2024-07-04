csharp
public partial class PaymentTransaction
{
    public virtual PaymentTransaction GetSpecificProcessingValues(PaymentTransaction paymentTransaction, Dictionary<string, object> processingValues)
    {
        if (paymentTransaction.ProviderCode != "razorpay")
        {
            return paymentTransaction;
        }

        if (paymentTransaction.Operation == "online_token" || paymentTransaction.Operation == "offline")
        {
            return paymentTransaction;
        }

        Dictionary<string, object> razorpaySpecificValues = new Dictionary<string, object>();
        razorpaySpecificValues["razorpay_key_id"] = paymentTransaction.ProviderId.GetPropertyValue("razorpay_key_id");
        razorpaySpecificValues["razorpay_customer_id"] = CreateRazorpayCustomer(paymentTransaction).GetPropertyValue("id");
        razorpaySpecificValues["is_tokenize_request"] = paymentTransaction.Tokenize;
        razorpaySpecificValues["razorpay_order_id"] = CreateRazorpayOrder(razorpaySpecificValues["razorpay_customer_id"].ToString()).GetPropertyValue("id");
        return paymentTransaction;
    }

    public virtual dynamic CreateRazorpayCustomer(PaymentTransaction paymentTransaction)
    {
        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload["name"] = paymentTransaction.PartnerName;
        payload["email"] = paymentTransaction.PartnerEmail;
        payload["contact"] = paymentTransaction.PartnerPhone;
        payload["fail_existing"] = "0";
        return paymentTransaction.ProviderId.MakeRequest("customers", payload);
    }

    public virtual string ValidatePhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone))
        {
            throw new Exception("Razorpay: The phone number is missing.");
        }

        try
        {
            return Env.CallMethod<string>("_phone_format", phone, this.PartnerCountryId, this.Tokenize);
        }
        catch (Exception)
        {
            throw new Exception("Razorpay: The phone number is invalid.");
        }
    }

    public virtual dynamic CreateRazorpayOrder(string customerId)
    {
        Dictionary<string, object> payload = PrepareOrderPayload(customerId);
        return this.ProviderId.MakeRequest("orders", payload);
    }

    public virtual Dictionary<string, object> PrepareOrderPayload(string customerId)
    {
        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload["amount"] = Env.CallMethod<long>("to_minor_currency_units", this.Amount, this.CurrencyId);
        payload["currency"] = this.CurrencyId.GetPropertyValue("name");

        string pmCode = (this.PaymentMethodId.GetPropertyValue("primary_payment_method_id") ?? this.PaymentMethodId).GetPropertyValue("code");
        if (!Env.Constant("FALLBACK_PAYMENT_METHOD_CODES").Contains(pmCode))
        {
            payload["method"] = pmCode;
        }

        if (this.Operation == "online_direct" || this.Operation == "validation")
        {
            payload["customer_id"] = customerId;

            if (this.Tokenize)
            {
                payload["token"] = new Dictionary<string, object>
                {
                    { "max_amount", Env.CallMethod<long>("to_minor_currency_units", GetMandateMaxAmount(), this.CurrencyId) },
                    { "expire_at", Env.CallMethod<long>("mktime", (DateTime.Now + new TimeSpan(10 * 365, 0, 0, 0)).ToUniversalTime().ToBinary()) },
                    { "frequency", "as_presented" }
                };
            }
        }
        else
        {
            payload["payment_capture"] = !this.ProviderId.GetPropertyValue("capture_manually");
        }

        if (this.ProviderId.GetPropertyValue("capture_manually"))
        {
            payload["payment"] = new Dictionary<string, object>
            {
                { "capture", "manual" },
                { "capture_options", new Dictionary<string, object>
                    {
                        { "manual_expiry_period", 7200 },
                        { "refund_speed", "normal" }
                    }
                }
            };
        }

        return payload;
    }

    public virtual double GetMandateMaxAmount()
    {
        string pmCode = (this.PaymentMethodId.GetPropertyValue("primary_payment_method_id") ?? this.PaymentMethodId).GetPropertyValue("code");
        double pmMaxAmountINR = Env.Constant("MANDATE_MAX_AMOUNT").GetPropertyValue<double>(pmCode);
        double pmMaxAmount = ConvertINRToCurrency(pmMaxAmountINR, this.CurrencyId);
        Dictionary<string, object> mandateValues = GetMandateValues();

        if (mandateValues.ContainsKey("amount") && mandateValues.ContainsKey("MRR"))
        {
            return Math.Min(pmMaxAmount, Math.Max(Convert.ToDouble(mandateValues["amount"]) * 1.5, Convert.ToDouble(mandateValues["MRR"]) * 5));
        }
        else
        {
            return pmMaxAmount;
        }
    }

    public virtual double ConvertINRToCurrency(double amount, Core.Currency currencyId)
    {
        Core.Currency inrCurrency = Env.SearchOne<Core.Currency>("name", "INR");
        return inrCurrency.Convert(amount, currencyId);
    }

    public virtual void SendPaymentRequest()
    {
        if (this.ProviderCode != "razorpay")
        {
            return;
        }

        if (this.TokenId == null)
        {
            throw new Exception("Razorpay: The transaction is not linked to a token.");
        }

        try
        {
            dynamic orderData = CreateRazorpayOrder(null);
            string phone = ValidatePhoneNumber(this.PartnerPhone);
            string[] providerRefParts = this.TokenId.ProviderRef.Split(',');
            string customerId = providerRefParts[0];
            string tokenId = providerRefParts[1];

            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["email"] = this.PartnerEmail;
            payload["contact"] = phone;
            payload["amount"] = orderData.GetPropertyValue("amount");
            payload["currency"] = this.CurrencyId.GetPropertyValue("name");
            payload["order_id"] = orderData.GetPropertyValue("id");
            payload["customer_id"] = customerId;
            payload["token"] = tokenId;
            payload["description"] = this.Reference;
            payload["recurring"] = "1";
            dynamic recurringPaymentData = this.ProviderId.MakeRequest("payments/create/recurring", payload);
            HandleNotificationData("razorpay", recurringPaymentData);
        }
        catch (Exception ex)
        {
            if (this.Operation == "offline")
            {
                SetError(ex.Message);
            }
            else
            {
                throw;
            }
        }
    }

    public virtual PaymentTransaction SendRefundRequest(double amountToRefund)
    {
        PaymentTransaction refundTx = Env.CallMethod<PaymentTransaction>("_send_refund_request", amountToRefund);
        if (this.ProviderCode != "razorpay")
        {
            return refundTx;
        }

        long convertedAmount = Env.CallMethod<long>("to_minor_currency_units", -refundTx.Amount, refundTx.CurrencyId);

        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload["amount"] = convertedAmount;
        payload["notes"] = new Dictionary<string, object> { { "reference", refundTx.Reference } };

        dynamic responseContent = this.ProviderId.MakeRequest($"payments/{this.ProviderReference}/refund", payload);
        responseContent.SetPropertyValue("entity_type", "refund");
        refundTx.HandleNotificationData("razorpay", responseContent);

        return refundTx;
    }

    public virtual PaymentTransaction SendCaptureRequest(double amountToCapture)
    {
        PaymentTransaction childCaptureTx = Env.CallMethod<PaymentTransaction>("_send_capture_request", amountToCapture);
        if (this.ProviderCode != "razorpay")
        {
            return childCaptureTx;
        }

        long convertedAmount = Env.CallMethod<long>("to_minor_currency_units", this.Amount, this.CurrencyId);
        Dictionary<string, object> payload = new Dictionary<string, object> { { "amount", convertedAmount }, { "currency", this.CurrencyId.GetPropertyValue("name") } };
        dynamic responseContent = this.ProviderId.MakeRequest($"payments/{this.ProviderReference}/capture", payload);
        HandleNotificationData("razorpay", responseContent);

        return childCaptureTx;
    }

    public virtual PaymentTransaction SendVoidRequest(double amountToVoid)
    {
        PaymentTransaction childVoidTx = Env.CallMethod<PaymentTransaction>("_send_void_request", amountToVoid);
        if (this.ProviderCode != "razorpay")
        {
            return childVoidTx;
        }

        throw new Exception("Transactions processed by Razorpay can't be manually voided from Odoo.");
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        PaymentTransaction tx = Env.CallMethod<PaymentTransaction>("_get_tx_from_notification_data", providerCode, notificationData);
        if (providerCode != "razorpay" || tx != null)
        {
            return tx;
        }

        string entityType = notificationData.GetPropertyValue<string>("entity_type", "payment");
        if (entityType == "payment")
        {
            string reference = notificationData.GetPropertyValue<string>("description");
            if (string.IsNullOrEmpty(reference))
            {
                throw new Exception("Razorpay: Received data with missing reference.");
            }
            tx = Env.Search<PaymentTransaction>(new Dictionary<string, object> { { "Reference", reference }, { "ProviderCode", "razorpay" } });
        }
        else
        {
            string reference = notificationData.GetPropertyValue<string>("notes", new Dictionary<string, object>())?.GetPropertyValue<string>("reference");
            if (!string.IsNullOrEmpty(reference))
            {
                tx = Env.Search<PaymentTransaction>(new Dictionary<string, object> { { "Reference", reference }, { "ProviderCode", "razorpay" } });
            }
            else
            {
                PaymentTransaction sourceTx = Env.SearchOne<PaymentTransaction>(new Dictionary<string, object> { { "ProviderReference", notificationData.GetPropertyValue<string>("payment_id") }, { "ProviderCode", "razorpay" } });
                if (sourceTx != null)
                {
                    tx = CreateRefundTxFromNotificationData(sourceTx, notificationData);
                }
            }
        }

        if (tx == null)
        {
            throw new Exception($"Razorpay: No transaction found matching reference {reference}.");
        }

        return tx;
    }

    public virtual PaymentTransaction CreateRefundTxFromNotificationData(PaymentTransaction sourceTx, Dictionary<string, object> notificationData)
    {
        string refundProviderReference = notificationData.GetPropertyValue<string>("id");
        long amountToRefund = notificationData.GetPropertyValue<long>("amount");
        if (string.IsNullOrEmpty(refundProviderReference) || amountToRefund == 0)
        {
            throw new Exception("Razorpay: Received incomplete refund data.");
        }

        double convertedAmount = Env.CallMethod<double>("to_major_currency_units", amountToRefund, sourceTx.CurrencyId);
        return sourceTx.CreateChildTransaction(convertedAmount, true, refundProviderReference);
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "razorpay")
        {
            return;
        }

        Dictionary<string, object> entityData = notificationData.ContainsKey("id") ? notificationData : this.ProviderId.MakeRequest($"payments/{notificationData.GetPropertyValue<string>("razorpay_payment_id")}", "GET");

        string entityId = entityData.GetPropertyValue<string>("id");
        if (string.IsNullOrEmpty(entityId))
        {
            throw new Exception("Razorpay: Received data with missing entity id.");
        }
        this.ProviderReference = entityId;

        string paymentMethodType = entityData.GetPropertyValue<string>("method", "");
        if (paymentMethodType == "card")
        {
            paymentMethodType = entityData.GetPropertyValue<string>("card", new Dictionary<string, object>())?.GetPropertyValue<string>("network").ToLower();
        }
        Payment.PaymentMethod paymentMethod = Env.CallMethod<Payment.PaymentMethod>("_get_from_code", paymentMethodType, Env.Constant("PAYMENT_METHODS_MAPPING"));
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;

        string entityStatus = entityData.GetPropertyValue<string>("status");
        if (string.IsNullOrEmpty(entityStatus))
        {
            throw new Exception("Razorpay: Received data with missing status.");
        }

        if (Env.Constant("PAYMENT_STATUS_MAPPING", "pending").Contains(entityStatus))
        {
            SetPending();
        }
        else if (Env.Constant("PAYMENT_STATUS_MAPPING", "authorized").Contains(entityStatus))
        {
            if (this.ProviderId.GetPropertyValue("capture_manually"))
            {
                SetAuthorized();
            }
        }
        else if (Env.Constant("PAYMENT_STATUS_MAPPING", "done").Contains(entityStatus))
        {
            if (this.TokenId == null && entityData.GetPropertyValue<string>("token_id") != null && this.ProviderId.GetPropertyValue("allow_tokenization"))
            {
                TokenizeFromNotificationData(entityData);
            }
            SetDone();
            if (this.Operation == "refund")
            {
                Env.CallMethod("trigger", "payment.cron_post_process_payment_tx");
            }
        }
        else if (Env.Constant("PAYMENT_STATUS_MAPPING", "error").Contains(entityStatus))
        {
            Env.LogWarning($"The transaction with reference {this.Reference} underwent an error. Reason: {entityData.GetPropertyValue<string>("error_description")}");
            SetError("An error occurred during the processing of your payment. Please try again.");
        }
        else
        {
            Env.LogWarning($"Received data for transaction with reference {this.Reference} with invalid payment status: {entityStatus}");
            SetError($"Razorpay: Received data with invalid status: {entityStatus}");
        }
    }

    public virtual void TokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        string pmCode = (this.PaymentMethodId.GetPropertyValue("primary_payment_method_id") ?? this.PaymentMethodId).GetPropertyValue("code");
        string details = pmCode == "card" ? notificationData.GetPropertyValue<string>("card", new Dictionary<string, object>())?.GetPropertyValue<string>("last4") :
            pmCode == "upi" ? notificationData.GetPropertyValue<string>("vpa")[notificationData.GetPropertyValue<string>("vpa").IndexOf('@') - 1:] : pmCode;

        Payment.PaymentToken token = Env.Create<Payment.PaymentToken>(new Dictionary<string, object>
        {
            { "ProviderId", this.ProviderId },
            { "PaymentMethodId", this.PaymentMethodId },
            { "PaymentDetails", details },
            { "PartnerId", this.PartnerId },
            { "ProviderRef", $"{notificationData.GetPropertyValue<string>("customer_id")},{notificationData.GetPropertyValue<string>("token_id")}" }
        });

        this.TokenId = token;
        this.Tokenize = false;
        Env.LogInfo($"Created token with id {token.Id} for partner with id {this.PartnerId.Id} from transaction with reference {this.Reference}");
    }

    public virtual Dictionary<string, object> GetMandateValues()
    {
        // Replace this with your logic for retrieving the linked document's values.
        return new Dictionary<string, object>();
    }

    public virtual void SetPending()
    {
        // Replace this with your logic for setting the transaction state to 'pending'.
    }

    public virtual void SetAuthorized()
    {
        // Replace this with your logic for setting the transaction state to 'authorized'.
    }

    public virtual void SetDone()
    {
        // Replace this with your logic for setting the transaction state to 'done'.
    }

    public virtual void SetError(string errorMessage)
    {
        // Replace this with your logic for setting the transaction state to 'error'.
    }

    public virtual void HandleNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        // Replace this with your logic for handling the notification data.
    }

    public virtual PaymentTransaction CreateChildTransaction(double amount, bool isRefund, string providerReference)
    {
        // Replace this with your logic for creating a child transaction.
        return null;
    }
}
