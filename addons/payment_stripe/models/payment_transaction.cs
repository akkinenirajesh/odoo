csharp
public partial class PaymentTransaction {
    public PaymentTransaction GetSpecificProcessingValues(Dictionary<string, object> processingValues) {
        if (this.ProviderCode != "stripe" || this.Operation == "online_token") {
            return base.GetSpecificProcessingValues(processingValues);
        }
        
        Dictionary<string, object> intent = this.StripeCreateIntent();
        string baseUrl = this.ProviderId.GetBaseUrl();
        return new Dictionary<string, object>() {
            { "client_secret", intent["client_secret"] },
            { "return_url", string.Format("{0}{1}?{2}", baseUrl, StripeController.ReturnUrl, 
            System.Web.HttpUtility.ParseQueryString(string.Format("reference={0}", this.Reference))) }
        };
    }

    public void SendPaymentRequest() {
        base.SendPaymentRequest();
        if (this.ProviderCode != "stripe") {
            return;
        }
        
        if (this.TokenId == null) {
            throw new UserError("Stripe: " + Env.Translate("The transaction is not linked to a token."));
        }
        
        // Make the payment request to Stripe
        Dictionary<string, object> paymentIntent = this.StripeCreateIntent();
        Env.Logger.Info(
            "payment request response for transaction with reference {0}:\n{1}", 
            this.Reference, paymentIntent.ToString()
        );
        
        if (paymentIntent == null) {
            return;
        }
        
        // Handle the payment request response
        Dictionary<string, object> notificationData = new Dictionary<string, object>() { { "reference", this.Reference } };
        StripeController.IncludePaymentIntentInNotificationData(paymentIntent, notificationData);
        this.HandleNotificationData("stripe", notificationData);
    }

    private Dictionary<string, object> StripeCreateIntent() {
        if (this.Operation == "validation") {
            return this.ProviderId.StripeMakeRequest("setup_intents", this.StripePrepareSetupIntentPayload());
        }
        
        // 'online_direct', 'online_token', 'offline'.
        return this.ProviderId.StripeMakeRequest(
            "payment_intents",
            this.StripePreparePaymentIntentPayload(),
            this.Operation == "offline",
            // Prevent multiple offline payments by token (e.g., due to a cursor rollback).
            this.Operation == "offline" ? payment_utils.GenerateIdempotencyKey(this, "payment_intents_token") : null
        );
    }
    
    private Dictionary<string, object> StripePrepareSetupIntentPayload() {
        Dictionary<string, object> customer = this.StripeCreateCustomer();
        Dictionary<string, object> setupIntentPayload = new Dictionary<string, object>() {
            { "customer", customer["id"] },
            { "description", this.Reference },
            { "payment_method_types[]", const.PAYMENT_METHODS_MAPPING.GetOrDefault(this.PaymentMethodCode, this.PaymentMethodCode) }
        };
        
        if (this.CurrencyId.Name in const.INDIAN_MANDATES_SUPPORTED_CURRENCIES) {
            setupIntentPayload = setupIntentPayload.Merge(this.StripePrepareMandateOptions());
        }
        return setupIntentPayload;
    }
    
    private Dictionary<string, object> StripePreparePaymentIntentPayload() {
        string paymentMethodType = this.PaymentMethodId.PrimaryPaymentMethodId.Code ?? this.PaymentMethodCode;
        Dictionary<string, object> paymentIntentPayload = new Dictionary<string, object>() {
            { "amount", payment_utils.ToMinorCurrencyUnits(this.Amount, this.CurrencyId) },
            { "currency", this.CurrencyId.Name.ToLower() },
            { "description", this.Reference },
            { "capture_method", this.ProviderId.CaptureManually ? "manual" : "automatic" },
            { "payment_method_types[]", const.PAYMENT_METHODS_MAPPING.GetOrDefault(paymentMethodType, paymentMethodType) },
            { "expand[]", "payment_method" },
        };
        paymentIntentPayload = paymentIntentPayload.Merge(stripe_utils.IncludeShippingAddress(this));
        
        if (this.Operation == "online_token" || this.Operation == "offline") {
            if (this.TokenId.StripePaymentMethod == null) {
                this.TokenId.StripeScaMigrateCustomer();
            }
            
            paymentIntentPayload = paymentIntentPayload.Merge(new Dictionary<string, object>() {
                { "confirm", true },
                { "customer", this.TokenId.ProviderRef },
                { "off_session", true },
                { "payment_method", this.TokenId.StripePaymentMethod },
                { "mandate", this.TokenId.StripeMandate ?? null }
            });
        } else {
            Dictionary<string, object> customer = this.StripeCreateCustomer();
            paymentIntentPayload["customer"] = customer["id"];
            if (this.Tokenize) {
                paymentIntentPayload["setup_future_usage"] = "off_session";
                if (this.CurrencyId.Name in const.INDIAN_MANDATES_SUPPORTED_CURRENCIES) {
                    paymentIntentPayload = paymentIntentPayload.Merge(this.StripePrepareMandateOptions());
                }
            }
        }
        return paymentIntentPayload;
    }

    private Dictionary<string, object> StripeCreateCustomer() {
        return this.ProviderId.StripeMakeRequest("customers", new Dictionary<string, object>() {
            { "address[city]", this.PartnerCity ?? null },
            { "address[country]", this.PartnerCountryId.Code ?? null },
            { "address[line1]", this.PartnerAddress ?? null },
            { "address[postal_code]", this.PartnerZip ?? null },
            { "address[state]", this.PartnerStateId.Name ?? null },
            { "description", string.Format("Odoo Partner: {0} (id: {1})", this.PartnerId.Name, this.PartnerId.Id) },
            { "email", this.PartnerEmail ?? null },
            { "name", this.PartnerName },
            { "phone", this.PartnerPhone != null ? this.PartnerPhone.Substring(0, Math.Min(this.PartnerPhone.Length, 20)) : null }
        });
    }
    
    private Dictionary<string, object> StripePrepareMandateOptions() {
        Dictionary<string, object> mandateValues = this.GetMandateValues();

        const string optionPathPrefix = "payment_method_options[card][mandate_options]";
        Dictionary<string, object> mandateOptions = new Dictionary<string, object>() {
            { $"{optionPathPrefix}[reference]", this.Reference },
            { $"{optionPathPrefix}[amount_type]", "maximum" },
            { $"{optionPathPrefix}[amount]", payment_utils.ToMinorCurrencyUnits(mandateValues.GetOrDefault("amount", 15000.0), this.CurrencyId) }, // Use the specified amount, if any, or define the maximum amount of 15.000 INR.
            { $"{optionPathPrefix}[start_date]", (long)Math.Round((mandateValues.GetOrDefault("start_datetime") ?? DateTime.Now).ToUniversalTime().ToUnixTimeSeconds()) },
            { $"{optionPathPrefix}[interval]", "sporadic" },
            { $"{optionPathPrefix}[supported_types][]", "india" }
        };
        
        if (mandateValues.ContainsKey("end_datetime")) {
            mandateOptions.Add($"{optionPathPrefix}[end_date]", (long)Math.Round(mandateValues["end_datetime"].ToUniversalTime().ToUnixTimeSeconds()));
        }
        
        if (mandateValues.ContainsKey("recurrence_unit") && mandateValues.ContainsKey("recurrence_duration")) {
            mandateOptions = mandateOptions.Merge(new Dictionary<string, object>() {
                { $"{optionPathPrefix}[interval]", mandateValues["recurrence_unit"] },
                { $"{optionPathPrefix}[interval_count]", mandateValues["recurrence_duration"] }
            });
        }
        
        if (this.Operation == "validation") {
            string currencyName = this.ProviderId.WithContext(new Dictionary<string, object>() { { "validation_pm", this.PaymentMethodId } })
            ._GetValidationCurrency().Name.ToLower();
            mandateOptions.Add($"{optionPathPrefix}[currency]", currencyName);
        }
        
        return mandateOptions;
    }
    
    public PaymentTransaction SendRefundRequest(decimal? amountToRefund = null) {
        PaymentTransaction refundTx = base.SendRefundRequest(amountToRefund);
        if (this.ProviderCode != "stripe") {
            return refundTx;
        }
        
        // Make the refund request to stripe.
        Dictionary<string, object> data = this.ProviderId.StripeMakeRequest(
            "refunds", new Dictionary<string, object>() {
                { "payment_intent", this.ProviderReference },
                { "amount", payment_utils.ToMinorCurrencyUnits(-refundTx.Amount, refundTx.CurrencyId) } // Refund transactions' amount is negative, inverse it.
            }
        );
        Env.Logger.Info(
            "Refund request response for transaction wih reference {0}:\n{1}", 
            this.Reference, data.ToString()
        );
        
        // Handle the refund request response.
        Dictionary<string, object> notificationData = new Dictionary<string, object>();
        StripeController.IncludeRefundInNotificationData(data, notificationData);
        refundTx.HandleNotificationData("stripe", notificationData);

        return refundTx;
    }
    
    public PaymentTransaction SendCaptureRequest(decimal? amountToCapture = null) {
        PaymentTransaction childCaptureTx = base.SendCaptureRequest(amountToCapture);
        if (this.ProviderCode != "stripe") {
            return childCaptureTx;
        }
        
        // Make the capture request to Stripe
        Dictionary<string, object> paymentIntent = this.ProviderId.StripeMakeRequest(
            string.Format("payment_intents/{0}/capture", this.ProviderReference)
        );
        Env.Logger.Info(
            "capture request response for transaction with reference {0}:\n{1}", 
            this.Reference, paymentIntent.ToString()
        );
        
        // Handle the capture request response
        Dictionary<string, object> notificationData = new Dictionary<string, object>() { { "reference", this.Reference } };
        StripeController.IncludePaymentIntentInNotificationData(paymentIntent, notificationData);
        this.HandleNotificationData("stripe", notificationData);
        
        return childCaptureTx;
    }
    
    public PaymentTransaction SendVoidRequest(decimal? amountToVoid = null) {
        PaymentTransaction childVoidTx = base.SendVoidRequest(amountToVoid);
        if (this.ProviderCode != "stripe") {
            return childVoidTx;
        }
        
        // Make the void request to Stripe
        Dictionary<string, object> paymentIntent = this.ProviderId.StripeMakeRequest(
            string.Format("payment_intents/{0}/cancel", this.ProviderReference)
        );
        Env.Logger.Info(
            "void request response for transaction with reference {0}:\n{1}", 
            this.Reference, paymentIntent.ToString()
        );
        
        // Handle the void request response
        Dictionary<string, object> notificationData = new Dictionary<string, object>() { { "reference", this.Reference } };
        StripeController.IncludePaymentIntentInNotificationData(paymentIntent, notificationData);
        this.HandleNotificationData("stripe", notificationData);
        
        return childVoidTx;
    }

    public PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData) {
        PaymentTransaction tx = base.GetTxFromNotificationData(providerCode, notificationData);
        if (providerCode != "stripe" || tx != null) {
            return tx;
        }

        string reference = notificationData.GetOrDefault<string>("reference");
        if (reference != null) {
            tx = Env.Model("Payment.PaymentTransaction").Search(new List<Tuple<string, object>>() { { "Reference", reference }, { "ProviderCode", "stripe" } });
        } else if (notificationData.GetOrDefault<string>("event_type") == "charge.refund.updated") {
            // The webhook notifications sent for `charge.refund.updated` events only contain a
            // refund object that has no 'description' (the merchant reference) field. We thus search
            // the transaction by its provider reference which is the refund id for refund txs.
            string refundId = notificationData["object_id"]; // The object is a refund.
            tx = Env.Model("Payment.PaymentTransaction").Search(new List<Tuple<string, object>>() { { "ProviderReference", refundId }, { "ProviderCode", "stripe" } });
        } else {
            throw new ValidationError("Stripe: " + Env.Translate("Received data with missing merchant reference"));
        }

        if (tx == null) {
            throw new ValidationError(
                "Stripe: " + Env.Translate("No transaction found matching reference {0}.", reference)
            );
        }
        return tx;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData) {
        base.ProcessNotificationData(notificationData);
        if (this.ProviderCode != "stripe") {
            return;
        }
        
        // Update the payment method.
        Dictionary<string, object> paymentMethod = notificationData.GetOrDefault<Dictionary<string, object>>("payment_method");
        if (paymentMethod != null) {
            // capture/void/refund requests receive a string.
            string paymentMethodType = paymentMethod.GetOrDefault<string>("type");
            if (this.PaymentMethodId.Code == paymentMethodType && paymentMethodType == "card") {
                paymentMethodType = paymentMethod["card"]["brand"];
            }
            paymentMethod = Env.Model("Payment.PaymentMethod")._GetFromCode(paymentMethodType, const.PAYMENT_METHODS_MAPPING);
            this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;
        }
        
        // Update the provider reference and the payment state.
        if (this.Operation == "validation") {
            this.ProviderReference = notificationData["setup_intent"]["id"];
            string status = notificationData["setup_intent"]["status"];
            this.SetStatus(status);
        } else if (this.Operation == "refund") {
            this.ProviderReference = notificationData["refund"]["id"];
            string status = notificationData["refund"]["status"];
            this.SetStatus(status);
        } else {
            // 'online_direct', 'online_token', 'offline'
            this.ProviderReference = notificationData["payment_intent"]["id"];
            string status = notificationData["payment_intent"]["status"];
            this.SetStatus(status);
        }
        
        if (this.Tokenize) {
            this.StripeTokenizeFromNotificationData(notificationData);
        }
        
        // Immediately post-process the transaction if it is a refund, as the post-processing
        // will not be triggered by a customer browsing the transaction from the portal.
        if (this.Operation == "refund") {
            Env.Model("Payment.PaymentTransaction").PostProcessPaymentTx();
        }
    }

    private void StripeTokenizeFromNotificationData(Dictionary<string, object> notificationData) {
        Dictionary<string, object> paymentMethod = notificationData.GetOrDefault<Dictionary<string, object>>("payment_method");
        if (paymentMethod == null) {
            Env.Logger.Warning("requested tokenization from notification data with missing payment method");
            return;
        }
        
        string mandate = null;
        // Extract the Stripe objects from the notification data.
        if (this.Operation == "online_direct") {
            string customerId = notificationData["payment_intent"]["customer"];
            Dictionary<string, object> chargesData = notificationData["payment_intent"]["charges"];
            Dictionary<string, object> paymentMethodDetails = chargesData["data"][0].GetOrDefault<Dictionary<string, object>>("payment_method_details");
            if (paymentMethodDetails != null) {
                mandate = paymentMethodDetails[paymentMethodDetails["type"]].GetOrDefault<string>("mandate");
            }
        } else {
            // 'validation'
            string customerId = notificationData["setup_intent"]["customer"];
        }
        
        // Another payment method (e.g., SEPA) might have been generated.
        if (paymentMethod[paymentMethod["type"]] == null) {
            Dictionary<string, object> paymentMethods = this.ProviderId.StripeMakeRequest(
                string.Format("customers/{0}/payment_methods", customerId), "GET"
            );
            Env.Logger.Info("Received payment_methods response:\n{0}", paymentMethods.ToString());
            paymentMethod = paymentMethods["data"][0];
        }
        
        // Create the token.
        PaymentToken token = Env.Model("Payment.PaymentToken").Create(new Dictionary<string, object>() {
            { "ProviderId", this.ProviderId.Id },
            { "PaymentMethodId", this.PaymentMethodId.Id },
            { "PaymentDetails", paymentMethod[paymentMethod["type"]].GetOrDefault<string>("last4") },
            { "PartnerId", this.PartnerId.Id },
            { "ProviderRef", customerId },
            { "StripePaymentMethod", paymentMethod["id"] },
            { "StripeMandate", mandate }
        });
        this.TokenId = token;
        this.Tokenize = false;
        Env.Logger.Info(
            "created token with id {0} for partner with id {1} from transaction with reference {2}", 
            token.Id, this.PartnerId.Id, this.Reference
        );
    }
    
    private void SetStatus(string status) {
        if (status == null) {
            throw new ValidationError("Stripe: " + Env.Translate("Received data with missing intent status."));
        }
        
        if (const.STATUS_MAPPING["draft"].Contains(status)) {
            // do nothing
        } else if (const.STATUS_MAPPING["pending"].Contains(status)) {
            this.SetPending();
        } else if (const.STATUS_MAPPING["authorized"].Contains(status)) {
            if (this.Tokenize) {
                this.StripeTokenizeFromNotificationData(new Dictionary<string, object>() { });
            }
            this.SetAuthorized();
        } else if (const.STATUS_MAPPING["done"].Contains(status)) {
            if (this.Tokenize) {
                this.StripeTokenizeFromNotificationData(new Dictionary<string, object>() { });
            }
            this.SetDone();
        } else if (const.STATUS_MAPPING["cancel"].Contains(status)) {
            this.SetCanceled();
        } else if (const.STATUS_MAPPING["error"].Contains(status)) {
            if (this.Operation != "refund") {
                Dictionary<string, object> lastPaymentError = notificationData.GetOrDefault<Dictionary<string, object>>("payment_intent").GetOrDefault<Dictionary<string, object>>("last_payment_error");
                if (lastPaymentError != null) {
                    string message = lastPaymentError.GetOrDefault<string>("message");
                } else {
                    string message = Env.Translate("The customer left the payment page.");
                }
                this.SetError(message);
            } else {
                this.SetError(Env.Translate(
                    "The refund did not go through. Please log into your Stripe Dashboard to get "
                    "more information on that matter, and address any accounting discrepancies."
                ), "done");
            }
        } else {
            // Classify unknown intent statuses as `error` tx state
            Env.Logger.Warning(
                "received invalid payment status ({0}) for transaction with reference {1}", 
                status, this.Reference
            );
            this.SetError(Env.Translate("Received data with invalid intent status: {0}", status));
        }
    }
    
    // ... other methods

    private Dictionary<string, object> GetMandateValues() {
        // ... implementation
    }

    private void SetPending() {
        // ... implementation
    }

    private void SetAuthorized() {
        // ... implementation
    }

    private void SetDone() {
        // ... implementation
    }

    private void SetCanceled() {
        // ... implementation
    }

    private void SetError(string message, string extraAllowedState = null) {
        // ... implementation
    }
}
