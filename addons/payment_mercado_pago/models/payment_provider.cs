csharp
public partial class PaymentProvider {
    public virtual PaymentProvider GetSupportedCurrencies() {
        var supportedCurrencies = Env.Call("PaymentProvider", "_GetSupportedCurrencies", this);
        if (this.Code == "mercado_pago") {
            supportedCurrencies = supportedCurrencies.Filter(c => (c.Name in const.SUPPORTED_CURRENCIES)).ToList();
        }
        return supportedCurrencies;
    }

    public virtual object MercadoPagoMakeRequest(string endpoint, object payload, string method = "POST") {
        var url = urls.UrlJoin("https://api.mercadopago.com", endpoint);
        var headers = new Dictionary<string, string> {
            { "Authorization", $"Bearer {this.MercadoPagoAccessToken}" }
        };
        try {
            if (method == "GET") {
                var response = requests.Get(url, payload, headers, 10);
                return response.Json();
            } else {
                var response = requests.Post(url, payload, headers, 10);
                response.RaiseForStatus();
                return response.Json();
            }
        } catch (requests.exceptions.HTTPError e) {
            _logger.Exception($"Invalid API request at {url} with data:\n{pprint.Pformat(payload)}", e);
            try {
                var responseContent = response.Json();
                var errorCode = responseContent.Get("error");
                var errorMessage = responseContent.Get("message");
                throw new ValidationError("Mercado Pago: " + _("The communication with the API failed. Mercado Pago gave us the following information: '%(error_message)s' (code %(error_code)s)", errorMessage, errorCode));
            } catch (ValueError) {
                throw new ValidationError("Mercado Pago: " + _("The communication with the API failed. The response is empty. Please verify your access token."));
            }
        } catch (requests.exceptions.ConnectionError | requests.exceptions.Timeout e) {
            _logger.Exception($"Unable to reach endpoint at {url}", e);
            throw new ValidationError("Mercado Pago: " + _("Could not establish the connection to the API."));
        }
    }

    public virtual object GetDefaultPaymentMethodCodes() {
        var defaultCodes = Env.Call("PaymentProvider", "_GetDefaultPaymentMethodCodes", this);
        if (this.Code != "mercado_pago") {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
