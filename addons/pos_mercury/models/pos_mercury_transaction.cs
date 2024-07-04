C#
public partial class PosMercury.MercuryTransaction {
    private int PosSessionId { get; set; }
    private int PaymentMethodId { get; set; }
    private string OperatorId { get; set; }
    private string MerchantId { get; set; }
    private string MerchantPassword { get; set; }
    private string Memo { get; set; }
    private bool IsVoidSale { get; set; }

    private PosSession GetPosSession() {
        var posSession = Env.Model("Pos.Session").SearchOne(new[] { ("State", "=", "opened"), ("UserId", "=", Env.Uid) });
        if (posSession == null) {
            throw new Exception("No opened point of sale session for user " + Env.User.Name + " found.");
        }
        posSession.Login();
        return posSession;
    }

    private PosMercuryConfig GetPosMercuryConfigId(PosConfig config, int paymentMethodId) {
        var paymentMethod = config.CurrentSessionId.PaymentMethodIds.Where(pm => pm.Id == paymentMethodId).FirstOrDefault();
        if (paymentMethod != null && paymentMethod.PosMercuryConfigId != null) {
            return paymentMethod.PosMercuryConfigId;
        }
        throw new Exception("No Vantiv configuration associated with the payment method.");
    }

    private void SetupRequest(Dictionary<string, object> data) {
        var posSession = GetPosSession();
        var config = posSession.ConfigId;
        var posMercuryConfig = GetPosMercuryConfigId(config, Convert.ToInt32(data["PaymentMethodId"]));

        data["OperatorId"] = posSession.UserId.Login;
        data["MerchantId"] = posMercuryConfig.MerchantId;
        data["MerchantPassword"] = posMercuryConfig.MerchantPassword;
        data["Memo"] = "Odoo " + Env.GetVersion("server_version");
    }

    private string DoRequest(string template, Dictionary<string, object> data) {
        if (string.IsNullOrEmpty(data["MerchantId"]) || string.IsNullOrEmpty(data["MerchantPassword"])) {
            return "not setup";
        }
        var xmlTransaction = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:mer=""http://www.mercurypay.com"">
  <soapenv:Header/>
  <soapenv:Body>
    <mer:CreditTransaction>
      <mer:tran>{Convert.ToString(Env.Render(template, data))}</mer:tran>
      <mer:pw>{data["MerchantPassword"]}</mer:pw>
    </mer:CreditTransaction>
  </soapenv:Body>
</soapenv:Envelope>";
        string response = "";

        var headers = new Dictionary<string, string>() {
            {"Content-Type", "text/xml"},
            {"SOAPAction", "http://www.mercurypay.com/CreditTransaction"}
        };

        string url = "https://w1.mercurypay.com/ws/ws.asmx";
        if (Env.GetParam("pos_mercury.enable_test_env") == "true") {
            url = "https://w1.mercurycert.net/ws/ws.asmx";
        }

        try {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(xmlTransaction, Encoding.UTF8, "text/xml");
            request.Headers.TryAddWithoutValidation("Content-Type", "text/xml");
            request.Headers.TryAddWithoutValidation("SOAPAction", "http://www.mercurypay.com/CreditTransaction");
            var client = new HttpClient();
            var responseMessage = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
            if (responseMessage.IsSuccessStatusCode) {
                response = WebUtility.HtmlDecode(responseMessage.Content.ReadAsStringAsync().Result);
            } else {
                response = "timeout";
            }
        } catch (Exception ex) {
            response = "timeout";
        }
        return response;
    }

    private string DoReversalOrVoidSale(Dictionary<string, object> data, bool isVoidSale) {
        try {
            SetupRequest(data);
        } catch (Exception) {
            return "internal error";
        }
        data["IsVoidSale"] = isVoidSale;
        var response = DoRequest("pos_mercury.mercury_voidsale", data);
        return response;
    }

    public string DoPayment(Dictionary<string, object> data) {
        try {
            SetupRequest(data);
        } catch (Exception) {
            return "internal error";
        }
        var response = DoRequest("pos_mercury.mercury_transaction", data);
        return response;
    }

    public string DoReversal(Dictionary<string, object> data) {
        return DoReversalOrVoidSale(data, false);
    }

    public string DoVoidSale(Dictionary<string, object> data) {
        return DoReversalOrVoidSale(data, true);
    }

    public string DoReturn(Dictionary<string, object> data) {
        try {
            SetupRequest(data);
        } catch (Exception) {
            return "internal error";
        }
        var response = DoRequest("pos_mercury.mercury_return", data);
        return response;
    }
}
