csharp
public partial class Pos.PosPaymentMethod
{
    public string VivaWalletMerchantId { get; set; }
    public string VivaWalletApiKey { get; set; }
    public string VivaWalletClientId { get; set; }
    public string VivaWalletClientSecret { get; set; }
    public string VivaWalletTerminalId { get; set; }
    public string VivaWalletBearerToken { get; set; }
    public string VivaWalletWebhookVerificationKey { get; set; }
    public string VivaWalletLatestResponse { get; set; }
    public bool VivaWalletTestMode { get; set; }
    public string VivaWalletWebhookEndpoint { get; set; }

    public void ComputeVivaWalletWebhookEndpoint()
    {
        this.VivaWalletWebhookEndpoint = $"{Env.GetBaseUrl()}/pos_viva_wallet/notification?company_id={this.CompanyId.Id}&token={this.VivaWalletWebhookVerificationKey}";
    }

    public bool IsWriteForbidden(List<string> fields)
    {
        List<string> whitelistedFields = new List<string>() { "VivaWalletBearerToken", "VivaWalletWebhookVerificationKey", "VivaWalletLatestResponse" };
        return base.IsWriteForbidden(fields.Except(whitelistedFields).ToList());
    }

    public List<Tuple<string, string>> GetPaymentTerminalSelection()
    {
        return base.GetPaymentTerminalSelection().Concat(new List<Tuple<string, string>>() { new Tuple<string, string>("viva_wallet", "Viva Wallet") }).ToList();
    }

    public Dictionary<string, string> BearerToken()
    {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user"))
        {
            throw new AccessError(_("Do not have access to fetch token from Viva Wallet"));
        }

        Dictionary<string, string> data = new Dictionary<string, string>() { { "grant_type", "client_credentials" } };
        try
        {
            var resp = Env.Http.Post(VivaWalletAccountGetEndpoint(), data, new HttpBasicAuth(this.VivaWalletClientId, this.VivaWalletClientSecret));
            string accessToken = resp.Json().GetString("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                this.VivaWalletBearerToken = accessToken;
                return new Dictionary<string, string>() { { "Authorization", $"Bearer {accessToken}" } };
            }
            else
            {
                throw new UserError(_("Not receive Bearer Token"));
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to call viva_wallet_bearer_token endpoint: {ex.Message}");
            return null;
        }
    }

    public string GetVerificationKey(string endpoint, string vivaWalletMerchantId, string vivaWalletApiKey)
    {
        if (tools.config["test_enable"])
        {
            return "viva_wallet_test";
        }

        try
        {
            var resp = Env.Http.Get($"{endpoint}/api/messages/config/token", new HttpBasicAuth(vivaWalletMerchantId, vivaWalletApiKey));
            return resp.Json().GetString("Key");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to call {endpoint}/api/messages/config/token endpoint: {ex.Message}");
            return null;
        }
    }

    public Dictionary<string, object> CallVivaWallet(string endpoint, string action, Dictionary<string, object> data = null)
    {
        try
        {
            var session = Env.Http.CreateSession();
            session.Headers.Add("Authorization", $"Bearer {this.VivaWalletBearerToken}");
            var resp = session.Request(action, $"{VivaWalletApiGetEndpoint()}/ecr/v1/{endpoint}", data);
            if (!string.IsNullOrEmpty(resp.Text) && resp.Json().GetString("detail") == "Could not validate credentials")
            {
                session.Headers.Update(BearerToken());
                resp = session.Request(action, $"{VivaWalletApiGetEndpoint()}/ecr/v1/{endpoint}", data);
            }

            if (resp.StatusCode == 200)
            {
                if (!string.IsNullOrEmpty(resp.Text))
                {
                    return resp.Json().ToObject<Dictionary<string, object>>();
                }
                return new Dictionary<string, object>() { { "success", resp.StatusCode } };
            }
            else
            {
                return new Dictionary<string, object>() { { "error", $"There are some issues between us and Viva Wallet, try again later. {resp.Json().GetString("detail")}" } };
            }
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>() { { "error", $"There are some issues between us and Viva Wallet, try again later. {ex.Message}" } };
        }
    }

    public void RetrieveSessionId(Dictionary<string, object> dataWebhook)
    {
        string[] sessionDetails = dataWebhook["MerchantTrns"].ToString().Split("/");
        string sessionId = sessionDetails[0];
        string posSessionId = sessionDetails[1];
        string endpoint = $"sessions/{sessionId}";
        Dictionary<string, object> data = CallVivaWallet(endpoint, "get");
        if (data.ContainsKey("success"))
        {
            data.Add("pos_session_id", posSessionId);
            data.Add("data_webhook", dataWebhook);
            this.VivaWalletLatestResponse = data.ToJson();
            SendNotification(data);
        }
        else
        {
            SendNotification(new Dictionary<string, object>() { { "error", $"There are some issues between us and Viva Wallet, try again later. {data["detail"]}" } });
        }
    }

    public void SendNotification(Dictionary<string, object> data)
    {
        var posSession = Env.Model("pos.session").Browse(int.Parse(data.GetValueOrDefault("pos_session_id", "0").ToString()));
        if (posSession != null)
        {
            Env.Bus.SendOne(posSession.GetBusChannelName(), "VIVA_WALLET_LATEST_RESPONSE", posSession.ConfigId.Id);
        }
    }

    public Dictionary<string, object> VivaWalletSendPaymentRequest(Dictionary<string, object> data)
    {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user"))
        {
            throw new AccessError(_("Only 'group_pos_user' are allowed to fetch token from Viva Wallet"));
        }

        string endpoint = "transactions:sale";
        return CallVivaWallet(endpoint, "post", data);
    }

    public Dictionary<string, object> VivaWalletSendPaymentCancel(Dictionary<string, object> data)
    {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user"))
        {
            throw new AccessError(_("Only 'group_pos_user' are allowed to fetch token from Viva Wallet"));
        }

        string sessionId = data["sessionId"].ToString();
        string cashRegisterId = data["cashRegisterId"].ToString();
        string endpoint = $"sessions/{sessionId}?cashRegisterId={cashRegisterId}";
        return CallVivaWallet(endpoint, "delete");
    }

    public void Write(Dictionary<string, object> vals)
    {
        base.Write(vals);

        if (vals.ContainsKey("VivaWalletMerchantId") && vals.ContainsKey("VivaWalletApiKey"))
        {
            this.VivaWalletWebhookVerificationKey = GetVerificationKey(VivaWalletWebhookGetEndpoint(), this.VivaWalletMerchantId, this.VivaWalletApiKey);
            if (string.IsNullOrEmpty(this.VivaWalletWebhookVerificationKey))
            {
                throw new UserError(_("Can't update payment method. Please check the data and update it."));
            }
        }
    }

    public void Create(Dictionary<string, object> vals)
    {
        base.Create(vals);
        if (this.VivaWalletMerchantId != null && this.VivaWalletApiKey != null)
        {
            this.VivaWalletWebhookVerificationKey = GetVerificationKey(VivaWalletWebhookGetEndpoint(), this.VivaWalletMerchantId, this.VivaWalletApiKey);
            if (string.IsNullOrEmpty(this.VivaWalletWebhookVerificationKey))
            {
                throw new UserError(_("Can't create payment method. Please check the data and update it."));
            }
        }
    }

    public Dictionary<string, object> GetLatestVivaWalletStatus()
    {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user"))
        {
            throw new AccessError(_("Only 'group_pos_user' are allowed to get latest transaction status"));
        }

        return this.VivaWalletLatestResponse.ToObject<Dictionary<string, object>>();
    }

    public void CheckVivaWalletCredentials()
    {
        if ((this.UsePaymentTerminal == "viva_wallet" &&
            (string.IsNullOrEmpty(this.VivaWalletMerchantId) ||
             string.IsNullOrEmpty(this.VivaWalletApiKey) ||
             string.IsNullOrEmpty(this.VivaWalletClientId) ||
             string.IsNullOrEmpty(this.VivaWalletClientSecret) ||
             string.IsNullOrEmpty(this.VivaWalletTerminalId))))
        {
            throw new UserError(_("It is essential to provide API key for the use of viva wallet"));
        }
    }

    private string VivaWalletAccountGetEndpoint()
    {
        return this.VivaWalletTestMode ? "https://demo-accounts.vivapayments.com" : "https://accounts.vivapayments.com";
    }

    private string VivaWalletApiGetEndpoint()
    {
        return this.VivaWalletTestMode ? "https://demo-api.vivapayments.com" : "https://api.vivapayments.com";
    }

    private string VivaWalletWebhookGetEndpoint()
    {
        return this.VivaWalletTestMode ? "https://demo.vivapayments.com" : "https://www.vivapayments.com";
    }
}
