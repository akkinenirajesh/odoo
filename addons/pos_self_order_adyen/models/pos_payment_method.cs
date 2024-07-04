csharp
public partial class Pos.PosPaymentMethod
{
    public bool Active { get; set; }
    public string AdyenTerminalIdentifier { get; set; }
    public string UsePaymentTerminal { get; set; }

    public object GetValidAcquirerData()
    {
        var res = Env.Call("Pos.PosPaymentMethod", "_get_valid_acquirer_data");
        res["metadata.self_order_id"] = Env.Call("Pos.PosPaymentMethod", "UNPREDICTABLE_ADYEN_DATA");
        return res;
    }

    public object PaymentRequestFromKiosk(object order)
    {
        if (UsePaymentTerminal != "adyen")
        {
            return Env.Call("Pos.PosPaymentMethod", "_payment_request_from_kiosk", order);
        }
        else
        {
            var posConfig = Env.Call(order, "session_id", "config_id");
            var randomNumber = Env.Call("Random", "randrange", 1000000000, 10000000000 - 1);

            var data = new
            {
                SaleToPOIRequest = new
                {
                    MessageHeader = new
                    {
                        ProtocolVersion = "3.0",
                        MessageClass = "Service",
                        MessageType = "Request",
                        MessageCategory = "Payment",
                        SaleID = $"{posConfig.Get("display_name")} (ID:{posConfig.Get("id")})",
                        ServiceID = randomNumber.ToString(),
                        POIID = AdyenTerminalIdentifier
                    },
                    PaymentRequest = new
                    {
                        SaleData = new
                        {
                            SaleTransactionID = new
                            {
                                TransactionID = order.Get("pos_reference"),
                                TimeStamp = DateTime.Now.ToUniversalTime().ToString("o")
                            },
                            SaleToAcquirerData = "metadata.self_order_id=" + order.Get("id").ToString()
                        },
                        PaymentTransaction = new
                        {
                            AmountsReq = new
                            {
                                Currency = order.Get("currency_id", "name"),
                                RequestedAmount = order.Get("amount_total")
                            }
                        }
                    }
                }
            };

            var req = ProxyAdyenRequest(data);

            return req != null && (req is bool || !(bool)req.Get("error"));
        }
    }

    private object ProxyAdyenRequest(object data)
    {
        // Implement Adyen Proxy request logic here
        return null;
    }

}
