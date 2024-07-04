csharp
public partial class PaymentTransaction 
{
    public virtual string GetSpecificRenderingValues(Dictionary<string, object> processingValues) 
    {
        if (this.ProviderCode != "mercado_pago") 
        {
            return processingValues.ToString(); 
        }

        var payload = MercadoPagoPreparePreferenceRequestPayload();
        Env.Log.Info(
            "Sending '/checkout/preferences' request for link creation:\n%s",
            payload.ToString()); 

        var apiUrl = this.ProviderId.MercadoPagoMakeRequest(
            "/checkout/preferences", payload).ToString(); 

        var parsedUrl = new System.Uri(apiUrl);
        var urlParams = System.Web.HttpUtility.ParseQueryString(parsedUrl.Query);

        return new Dictionary<string, object> 
        {
            { "api_url", apiUrl },
            { "url_params", urlParams }, 
        }.ToString();
    }

    public virtual Dictionary<string, object> MercadoPagoPreparePreferenceRequestPayload() 
    {
        var baseUrl = this.ProviderId.GetBaseUrl();
        var returnUrl = new System.Uri(baseUrl, MercadoPagoController.ReturnUrl); 
        var sanitizedReference = System.Web.HttpUtility.UrlEncode(this.Reference);
        var webhookUrl = new System.Uri(
            baseUrl, $"{MercadoPagoController.WebhookUrl}/{sanitizedReference}"); 

        var unitPrice = this.Amount;
        if (this.CurrencyId.Name == "CLP" || this.CurrencyId.Name == "COP") 
        {
            var roundedUnitPrice = Convert.ToInt32(this.Amount); 
            if (roundedUnitPrice != this.Amount)
            {
                throw new Exception(
                    $"Prices in the currency {this.CurrencyId.Name} must be expressed in integer values."); 
            }
            unitPrice = roundedUnitPrice;
        }

        return new Dictionary<string, object> 
        {
            { "auto_return", "all" },
            { "back_urls", new Dictionary<string, string> 
                {
                    { "success", returnUrl.ToString() }, 
                    { "pending", returnUrl.ToString() }, 
                    { "failure", returnUrl.ToString() } 
                } 
            },
            { "external_reference", this.Reference },
            { "items", new List<Dictionary<string, object>> 
                {
                    new Dictionary<string, object> 
                    {
                        { "title", this.Reference }, 
                        { "quantity", 1 }, 
                        { "currency_id", this.CurrencyId.Name }, 
                        { "unit_price", unitPrice } 
                    } 
                } 
            },
            { "notification_url", webhookUrl.ToString() }, 
            { "payer", new Dictionary<string, object> 
                {
                    { "name", this.PartnerName }, 
                    { "email", this.PartnerEmail }, 
                    { "phone", new Dictionary<string, string> 
                        {
                            { "number", this.PartnerPhone }
                        }
                    },
                    { "address", new Dictionary<string, string> 
                        {
                            { "zip_code", this.PartnerZip }, 
                            { "street_name", this.PartnerAddress }
                        } 
                    }
                }
            },
            { "payment_methods", new Dictionary<string, int> 
                {
                    { "installments", 1 } 
                } 
            }
        };
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData) 
    {
        if (providerCode != "mercado_pago") 
        {
            return Env.GetModel<PaymentTransaction>().Search(
                new[] { ("ProviderCode", "=", providerCode) });
        }

        var reference = notificationData.GetValueOrDefault<string>("external_reference"); 
        if (string.IsNullOrEmpty(reference))
        {
            throw new Exception("Mercado Pago: Received data with missing reference.");
        }

        var tx = Env.GetModel<PaymentTransaction>().Search(
            new[] { ("Reference", "=", reference), ("ProviderCode", "=", "mercado_pago") });

        if (tx == null) 
        {
            throw new Exception($"Mercado Pago: No transaction found matching reference {reference}.");
        }
        return tx;
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData) 
    {
        if (this.ProviderCode != "mercado_pago") 
        {
            return;
        }

        // Update the provider reference.
        var paymentId = notificationData.GetValueOrDefault<string>("payment_id"); 
        if (string.IsNullOrEmpty(paymentId)) 
        {
            throw new Exception("Mercado Pago: Received data with missing payment id.");
        }
        this.ProviderReference = paymentId;

        // Verify the notification data.
        var verifiedPaymentData = this.ProviderId.MercadoPagoMakeRequest(
            $"/v1/payments/{this.ProviderReference}", "GET").ToString(); 

        // Update the payment method.
        var paymentMethodType = verifiedPaymentData.GetValueOrDefault<string>("payment_type_id", "");
        var odooCode = const.PAYMENT_METHODS_MAPPING
            .FirstOrDefault(x => x.Value.Split(',').Contains(paymentMethodType)).Key;
        var paymentMethod = Env.GetModel<PaymentMethod>().GetFromCode(
            odooCode, const.PAYMENT_METHODS_MAPPING); 

        // Fall back to "unknown" if the payment method is not found (and if "unknown" is found), as
        // the user might have picked a different payment method than on Odoo's payment form.
        if (paymentMethod == null) 
        {
            paymentMethod = Env.GetModel<PaymentMethod>().Search(
                new[] { ("Code", "=", "unknown") }).FirstOrDefault(); 
        }
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;

        // Update the payment state.
        var paymentStatus = verifiedPaymentData.GetValueOrDefault<string>("status");
        if (string.IsNullOrEmpty(paymentStatus)) 
        {
            throw new Exception("Mercado Pago: Received data with missing status.");
        }

        if (const.TRANSACTION_STATUS_MAPPING["pending"].Contains(paymentStatus)) 
        {
            SetPending();
        }
        else if (const.TRANSACTION_STATUS_MAPPING["done"].Contains(paymentStatus)) 
        {
            SetDone();
        }
        else if (const.TRANSACTION_STATUS_MAPPING["canceled"].Contains(paymentStatus)) 
        {
            SetCancelled();
        }
        else if (const.TRANSACTION_STATUS_MAPPING["error"].Contains(paymentStatus)) 
        {
            var statusDetail = verifiedPaymentData.GetValueOrDefault<string>("status_detail");
            Env.Log.Warning(
                "Received data for transaction with reference {0} with status {1} and error code: {2}",
                this.Reference, paymentStatus, statusDetail); 
            var errorMessage = MercadoPagoGetErrorMsg(statusDetail);
            SetError(errorMessage);
        }
        else 
        {
            Env.Log.Warning(
                "Received data for transaction with reference {0} with invalid payment status: {1}",
                this.Reference, paymentStatus);
            SetError($"Mercado Pago: Received data with invalid status: {paymentStatus}");
        }
    }

    public virtual string MercadoPagoGetErrorMsg(string statusDetail) 
    {
        return $"Mercado Pago: {const.ERROR_MESSAGE_MAPPING.GetValueOrDefault(statusDetail, const.ERROR_MESSAGE_MAPPING["cc_rejected_other_reason"])}";
    }
}
