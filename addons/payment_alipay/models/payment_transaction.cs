csharp
public partial class PaymentTransaction {
    public virtual string GetSpecificRenderingValues(Dictionary<string, object> processingValues) {
        if (this.ProviderCode != "alipay") {
            return Env.Call<string>("payment.transaction", "_get_specific_rendering_values", processingValues);
        }

        string base_url = this.ProviderId.GetBaseUrl();
        Dictionary<string, string> renderingValues = new Dictionary<string, string>() {
            {"_input_charset", "utf-8"},
            {"notify_url", urls.url_join(base_url, AlipayController._webhook_url)},
            {"out_trade_no", this.Reference},
            {"partner", this.ProviderId.AlipayMerchantPartnerId},
            {"return_url", urls.url_join(base_url, AlipayController._return_url)},
            {"subject", this.Reference},
            {"total_fee", string.Format("{0:0.00}", this.Amount)}
        };

        if (this.ProviderId.AlipayPaymentMethod == "standard_checkout") {
            renderingValues.Add("service", "create_forex_trade");
            renderingValues.Add("product_code", "NEW_OVERSEAS_SELLER");
            renderingValues.Add("currency", this.Currency.Name);
        } else {
            renderingValues.Add("service", "create_direct_pay_by_user");
            renderingValues.Add("payment_type", "1");
            renderingValues.Add("seller_email", this.ProviderId.AlipaySellerEmail);
        }

        string sign = this.ProviderId.ComputeAlipaySignature(renderingValues);
        renderingValues.Add("sign_type", "MD5");
        renderingValues.Add("sign", sign);
        renderingValues.Add("api_url", this.ProviderId.GetAlipayApiUrl());
        return renderingValues;
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData) {
        if (providerCode != "alipay" || notificationData == null) {
            return Env.Call<PaymentTransaction>("payment.transaction", "_get_tx_from_notification_data", providerCode, notificationData);
        }

        string reference = notificationData.GetValueOrDefault("reference") as string ?? notificationData.GetValueOrDefault("out_trade_no") as string;
        string txnId = notificationData.GetValueOrDefault("trade_no") as string;
        if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(txnId)) {
            throw new ValidationException(
                "Alipay: " + 
                "Received data with missing reference %(r)s or txn_id %(t)s.", 
                new Dictionary<string, string>() {
                    {"r", reference}, {"t", txnId}
                }
            );
        }

        PaymentTransaction tx = Env.Search<PaymentTransaction>(new[] {
            new Tuple<string, object>("Reference", reference),
            new Tuple<string, object>("ProviderCode", "alipay")
        });
        if (tx == null) {
            throw new ValidationException(
                "Alipay: " + _("No transaction found matching reference %s.", reference)
            );
        }

        return tx;
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData) {
        if (notificationData == null) {
            return;
        }

        Env.Call("payment.transaction", "_process_notification_data", notificationData);
        if (this.ProviderCode != "alipay") {
            return;
        }

        this.ProviderReference = notificationData.GetValueOrDefault("trade_no") as string;
        string status = notificationData.GetValueOrDefault("trade_status") as string;
        if (status == "TRADE_FINISHED" || status == "TRADE_SUCCESS") {
            this.SetDone();
        } else if (status == "TRADE_CLOSED") {
            this.SetCanceled();
        } else {
            _logger.info(
                "received data with invalid payment status (%s) for transaction with reference %s",
                status, this.Reference,
            );
            this.SetError("Alipay: " + _("received invalid transaction status: %s", status));
        }
    }

    private void SetDone() {
        // Implementation to set the transaction state to 'done'
    }

    private void SetCanceled() {
        // Implementation to set the transaction state to 'cancel'
    }

    private void SetError(string message) {
        // Implementation to set the transaction state to 'error' with the given message
    }
}
