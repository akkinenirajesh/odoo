csharp
public partial class PaymentTransaction
{
    public string Reference { get; set; }
    public string ProviderCode { get; set; }
    public string ProviderReference { get; set; }
    public double Amount { get; set; }
    public Core.Currency Currency { get; set; }
    public Res.Partner PartnerId { get; set; }
    public string PartnerLang { get; set; }
    public Payment.PaymentMethod PaymentMethodId { get; set; }
    public Payment.TransactionState State { get; set; }
    public Payment.Acquirer AcquirerId { get; set; }
    public DateTime Date { get; set; }
    public Payment.TransactionOperation Operation { get; set; }
    public Payment.PaymentToken PaymentTokenId { get; set; }
    public string ProviderSpecificValues { get; set; }
    public string TransactionValues { get; set; }
    public string AccessKey { get; set; }
    public string Signature { get; set; }
    public string PaymentOption { get; set; }
    public string FortId { get; set; }
    public string ResponseMessage { get; set; }
    public string Status { get; set; }
    public bool IsProcessed { get; set; }
    public string ProcessingValues { get; set; }

    public void ComputeReference(string providerCode, string prefix, string separator)
    {
        // TODO: Implement logic
    }

    public void GetSpecificRenderingValues(string processingValues)
    {
        // TODO: Implement logic
    }

    public void GetTxFromNotificationData(string providerCode, string notificationData)
    {
        // TODO: Implement logic
    }

    public void ProcessNotificationData(string notificationData)
    {
        // TODO: Implement logic
    }

    public void SetPending()
    {
        // TODO: Implement logic
    }

    public void SetDone()
    {
        // TODO: Implement logic
    }

    public void SetError(string message)
    {
        // TODO: Implement logic
    }
}
