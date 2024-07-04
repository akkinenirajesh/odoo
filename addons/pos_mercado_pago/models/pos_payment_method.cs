csharp
public partial class PosPaymentMethod {
    public void ForcePdv() {
        if (!Env.User.HasGroup("PointOfSale.GroupPosUser")) {
            throw new AccessError("Do not have access to fetch token from Mercado Pago");
        }

        var mercadoPago = new MercadoPagoPosRequest(this.MpBearerToken);
        _logger.Info("Calling Mercado Pago to force the terminal mode to \"PDV\"");

        var mode = new { operating_mode = "PDV" };
        var resp = mercadoPago.CallMercadoPago("patch", $"point/integration-api/devices/{this.MpIdPointSmartComplet}", mode);
        if (resp.Get("operating_mode") != "PDV") {
            throw new UserError($"Unexpected Mercado Pago response: {resp}");
        }
        _logger.Debug("Successfully set the terminal mode to 'PDV'.");
    }

    public object MpPaymentIntentCreate(object infos) {
        if (!Env.User.HasGroup("PointOfSale.GroupPosUser")) {
            throw new AccessError("Do not have access to fetch token from Mercado Pago");
        }

        var mercadoPago = new MercadoPagoPosRequest(this.MpBearerToken);
        // Call Mercado Pago for payment intend creation
        var resp = mercadoPago.CallMercadoPago("post", $"point/integration-api/devices/{this.MpIdPointSmartComplet}/payment-intents", infos);
        _logger.Debug("MpPaymentIntentCreate(), response from Mercado Pago: {resp}");
        return resp;
    }

    public object MpPaymentIntentGet(string paymentIntentId) {
        if (!Env.User.HasGroup("PointOfSale.GroupPosUser")) {
            throw new AccessError("Do not have access to fetch token from Mercado Pago");
        }

        var mercadoPago = new MercadoPagoPosRequest(this.MpBearerToken);
        // Call Mercado Pago for payment intend status
        var resp = mercadoPago.CallMercadoPago("get", $"point/integration-api/payment-intents/{paymentIntentId}", new {});
        _logger.Debug("MpPaymentIntentGet(), response from Mercado Pago: {resp}");
        return resp;
    }

    public object MpPaymentIntentCancel(string paymentIntentId) {
        if (!Env.User.HasGroup("PointOfSale.GroupPosUser")) {
            throw new AccessError("Do not have access to fetch token from Mercado Pago");
        }

        var mercadoPago = new MercadoPagoPosRequest(this.MpBearerToken);
        // Call Mercado Pago for payment intend cancelation
        var resp = mercadoPago.CallMercadoPago("delete", $"point/integration-api/devices/{this.MpIdPointSmartComplet}/payment-intents/{paymentIntentId}", new {});
        _logger.Debug("MpPaymentIntentCancel(), response from Mercado Pago: {resp}");
        return resp;
    }

    private string FindTerminal(string token, string pointSmart) {
        var mercadoPago = new MercadoPagoPosRequest(token);
        var data = mercadoPago.CallMercadoPago("get", "point/integration-api/devices", new {});
        if (data.ContainsKey("devices")) {
            // Search for a device id that contains the serial number entered by the user
            var foundDevice = data["devices"].FirstOrDefault(device => pointSmart.Contains(device["id"]));

            if (foundDevice == null) {
                throw new UserError("The terminal serial number is not registered on Mercado Pago");
            }

            return foundDevice.Get("id", "");
        } else {
            throw new UserError("Please verify your production user token as it was rejected");
        }
    }

    public void Write(object vals) {
        if (vals.ContainsKey("MpIdPointSmart") || vals.ContainsKey("MpBearerToken")) {
            this.MpIdPointSmartComplet = FindTerminal(this.MpBearerToken, this.MpIdPointSmart);
        }
    }

    public void Create(object vals) {
        if (this.MpBearerToken != null) {
            this.MpIdPointSmartComplet = FindTerminal(this.MpBearerToken, this.MpIdPointSmart);
        }
    }
}
