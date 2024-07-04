C#
public partial class PaymentTransaction {
    public decimal GetSpecificProcessingValues(decimal amount, Core.Currency currencyId, Res.Partner partnerId)
    {
        // C# code implementation
        return amount;
    }

    public void SendPaymentRequest()
    {
        // C# code implementation
    }

    public void SendRefundRequest(decimal amountToRefund)
    {
        // C# code implementation
    }

    public void SendCaptureRequest(decimal amountToCapture)
    {
        // C# code implementation
    }

    public void SendVoidRequest(decimal amountToVoid)
    {
        // C# code implementation
    }

    public PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        // C# code implementation
        return new PaymentTransaction();
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        // C# code implementation
    }

    public void TokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        // C# code implementation
    }

    public void _SetPending()
    {
        // C# code implementation
    }

    public void _SetDone()
    {
        // C# code implementation
    }

    public void _SetAuthorized()
    {
        // C# code implementation
    }

    public void _SetCanceled()
    {
        // C# code implementation
    }

    public void _SetError(string message)
    {
        // C# code implementation
    }

    public void _LogMessageOnLinkedDocuments(string message)
    {
        // C# code implementation
    }
}
