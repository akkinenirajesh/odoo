csharp
public partial class PaymentTransaction
{
    public virtual string GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "custom")
        {
            return Env.Call("Payment.PaymentTransaction", "_get_specific_rendering_values", this, processingValues);
        }

        return $"{{'api_url': '{CustomController._process_url}', 'reference': '{this.Reference}'}}";
    }

    public virtual string GetCommunication()
    {
        if (this.InvoiceIds.Count > 0)
        {
            return this.InvoiceIds[0].PaymentReference;
        }
        else if (this.SaleOrderIds.Count > 0)
        {
            return this.SaleOrderIds[0].Reference;
        }

        return this.Reference;
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "custom" || Env.Call<List<PaymentTransaction>>("Payment.PaymentTransaction", "_get_tx_from_notification_data", this, providerCode, notificationData).Count == 1)
        {
            return Env.Call<PaymentTransaction>("Payment.PaymentTransaction", "_get_tx_from_notification_data", this, providerCode, notificationData);
        }

        string reference = notificationData["reference"] as string;
        List<PaymentTransaction> tx = Env.Search<PaymentTransaction>("Payment.PaymentTransaction", new Dictionary<string, object> { { "Reference", reference }, { "ProviderCode", "custom" } });
        if (tx.Count == 0)
        {
            throw new Exception($"Wire Transfer: No transaction found matching reference {reference}.");
        }

        return tx[0];
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        Env.Call("Payment.PaymentTransaction", "_process_notification_data", this, notificationData);
        if (this.ProviderCode != "custom")
        {
            return;
        }

        Env.Log("validated custom payment for transaction with reference {this.Reference}: set as pending");
        this.SetPending();
    }

    public virtual void LogReceivedMessage()
    {
        if (this.ProviderCode != "custom")
        {
            Env.Call("Payment.PaymentTransaction", "_log_received_message", this);
        }
    }

    public virtual string GetSentMessage()
    {
        string message = Env.Call<string>("Payment.PaymentTransaction", "_get_sent_message", this);
        if (this.ProviderCode == "custom")
        {
            message = $"The customer has selected {this.ProviderId.Name} to make the payment.";
        }
        return message;
    }

    public virtual void SetPending()
    {
        // Implement this method using your own logic.
    }
}
