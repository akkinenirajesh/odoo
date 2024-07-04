csharp
public partial class PosPayment
{
    public virtual void UpdatePaymentLineForTip(decimal tipAmount)
    {
        var res = base.UpdatePaymentLineForTip(tipAmount);
        if (this.PaymentMethodId.UsePaymentTerminal == "adyen")
        {
            this.AdyenCapture();
        }
        return res;
    }

    public virtual void AdyenCapture()
    {
        var data = new {
            originalReference = this.TransactionId,
            modificationAmount = new {
                value = (long)(this.Amount * Math.Pow(10, this.CurrencyId.DecimalPlaces)),
                currency = this.CurrencyId.Name,
            },
            merchantAccount = this.PaymentMethodId.AdyenMerchantAccount
        };

        this.PaymentMethodId.ProxyAdyenRequest(data, "capture");
    }
}

public partial class PaymentMethod
{
    public virtual string ProxyAdyenRequest(object data, string action)
    {
        // Implement your own logic for making the Adyen request here.
        // You can use any C# library for making HTTP requests, like HttpClient.
        // The "data" object should be serialized to JSON before sending the request.
        // Example using HttpClient:
        using (var client = new HttpClient())
        {
            var requestUri = new Uri("https://your-adyen-api-endpoint/" + action);
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = client.PostAsync(requestUri, content).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                // Process the Adyen response here.
                return responseContent;
            }
            else
            {
                // Handle the error.
                throw new Exception("Error making Adyen request.");
            }
        }
    }
}
