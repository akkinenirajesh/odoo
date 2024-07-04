csharp
public partial class PaymentProvider
{
    public void ComputeFeatureSupportFields()
    {
        if (this.Code == "stripe")
        {
            this.SupportExpressCheckout = true;
            this.SupportManualCapture = "full_only";
            this.SupportRefund = "partial";
            this.SupportTokenization = true;
        }
    }

    public bool StripeHasConnectedAccount()
    {
        return false;
    }

    public bool StripeOnboardingIsOngoing()
    {
        return false;
    }

    public dynamic ActionStripeConnectAccount(int? menuId = null)
    {
        if (Env.Company.Country.Code not in StripeConstants.SupportedCountries)
        {
            throw new RedirectWarning("Stripe Connect is not available in your country, please use another payment provider.", Env.Ref("payment.action_payment_provider").Id, "Other Payment Providers");
        }

        if (this.State == "enabled")
        {
            Env["onboarding.onboarding.step"].ActionValidateStepPaymentProvider();
            return new { type = "ir.actions.act_window_close" };
        }

        var connectedAccount = StripeFetchOrCreateConnectedAccount();

        var accountLinkUrl = StripeCreateAccountLink(connectedAccount["id"], menuId);

        if (accountLinkUrl != null)
        {
            return new { type = "ir.actions.act_url", url = accountLinkUrl, target = "self" };
        }

        return new { type = "ir.actions.act_window", model = "payment.provider", views = new[] { new[] { false, "form" } }, resId = this.Id };
    }

    public dynamic ActionStripeCreateWebhook()
    {
        if (this.StripeWebhookSecret != null)
        {
            return new { type = "ir.actions.client", tag = "display_notification", params = new { message = "Your Stripe Webhook is already set up.", sticky = false, type = "warning", next = new { type = "ir.actions.act_window_close" } } };
        }

        if (this.StripeSecretKey == null)
        {
            return new { type = "ir.actions.client", tag = "display_notification", params = new { message = "You cannot create a Stripe Webhook if your Stripe Secret Key is not set.", sticky = false, type = "danger", next = new { type = "ir.actions.act_window_close" } } };
        }

        var webhook = StripeMakeRequest("webhook_endpoints", new { url = GetStripeWebhookUrl(), enabled_events = new[] { StripeConstants.HandledWebhookEvents }, api_version = StripeConstants.ApiVersion });
        this.StripeWebhookSecret = webhook.secret;
        return new { type = "ir.actions.client", tag = "display_notification", params = new { message = "You Stripe Webhook was successfully set up!", sticky = false, type = "info", next = new { type = "ir.actions.act_window_close" } } };
    }

    public dynamic ActionStripeVerifyApplePayDomain()
    {
        if (this.State == "test")
        {
            throw new UserError("Please use live credentials to enable Apple Pay.");
        }

        var webDomain = new Uri(Env.Company.BaseUrl).Host;
        var responseContent = StripeMakeRequest("apple_pay/domains", new { domain_name = webDomain });
        if (!responseContent.livemode)
        {
            throw new UserError("Please use live credentials to enable Apple Pay.");
        }

        return new { type = "ir.actions.client", tag = "display_notification", params = new { message = "Your web domain was successfully verified.", type = "success" } };
    }

    public string GetStripeWebhookUrl()
    {
        return new UriBuilder(Env.Company.BaseUrl).Path = StripeController.WebhookUrl;
    }

    public dynamic StripeMakeRequest(string endpoint, dynamic payload = null, string method = "POST", bool offline = false, string idempotencyKey = null)
    {
        var url = new UriBuilder("https://api.stripe.com/v1/") { Path = endpoint }.ToString();
        var headers = new Dictionary<string, string>
        {
            { "AUTHORIZATION", $"Bearer {StripeUtils.GetSecretKey(this)}" },
            { "Stripe-Version", StripeConstants.ApiVersion },
        };
        headers.AddRange(GetStripeExtraRequestHeaders());

        if (method == "POST" && idempotencyKey != null)
        {
            headers.Add("Idempotency-Key", idempotencyKey);
        }

        try
        {
            var response = new HttpClient().PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), headers).Result;

            if (!response.IsSuccessStatusCode && !offline && response.StatusCode >= HttpStatusCode.BadRequest && response.StatusCode < HttpStatusCode.InternalServerError)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                var error = JsonConvert.DeserializeObject<Dictionary<string, object>>(content).GetValueOrDefault("error");

                if (error != null)
                {
                    throw new ValidationError($"Stripe: The communication with the API failed.\nStripe gave us the following info about the problem:\n'{error["message"]}'");
                }
            }

            return response.Content.ReadAsAsync<dynamic>().Result;
        }
        catch (HttpRequestException e)
        {
            throw new ValidationError($"Stripe: Could not establish the connection to the API.");
        }
        catch (Exception e)
        {
            throw new ValidationError($"Stripe: The communication with the API failed.\n'{e.Message}'");
        }
    }

    public Dictionary<string, string> GetStripeExtraRequestHeaders()
    {
        return new Dictionary<string, string>();
    }

    public dynamic StripeFetchOrCreateConnectedAccount()
    {
        return StripeMakeProxyRequest("accounts", StripePrepareConnectAccountPayload());
    }

    public dynamic StripePrepareConnectAccountPayload()
    {
        return new
        {
            type = "standard",
            country = StripeGetCountry(Env.Company.Country.Code),
            email = Env.Company.Email,
            business_type = "individual",
            company = new
            {
                address = new
                {
                    city = Env.Company.City,
                    country = StripeGetCountry(Env.Company.Country.Code),
                    line1 = Env.Company.Street,
                    line2 = Env.Company.Street2,
                    postal_code = Env.Company.Zip,
                    state = Env.Company.State.Name
                },
                name = Env.Company.Name
            },
            individual = new
            {
                address = new
                {
                    city = Env.Company.City,
                    country = StripeGetCountry(Env.Company.Country.Code),
                    line1 = Env.Company.Street,
                    line2 = Env.Company.Street2,
                    postal_code = Env.Company.Zip,
                    state = Env.Company.State.Name
                },
                email = Env.Company.Email
            },
            business_profile = new
            {
                name = Env.Company.Name
            }
        };
    }

    public string StripeCreateAccountLink(string connectedAccountId, int? menuId)
    {
        var baseUrl = Env.Company.BaseUrl;
        var returnUrl = StripeController.OnboardingReturnUrl;
        var refreshUrl = StripeController.OnboardingRefreshUrl;
        var returnParams = new Dictionary<string, string> { { "provider_id", this.Id.ToString() }, { "menu_id", menuId?.ToString() ?? "" } };
        var refreshParams = new Dictionary<string, string>(returnParams) { { "account_id", connectedAccountId } };

        var accountLink = StripeMakeProxyRequest("account_links", new
        {
            account = connectedAccountId,
            return_url = $"{new UriBuilder(baseUrl).Path = returnUrl}.{new UriBuilder(baseUrl).Query = "?" + string.Join("&", returnParams.Select(x => $"{x.Key}={x.Value}"))}",
            refresh_url = $"{new UriBuilder(baseUrl).Path = refreshUrl}.{new UriBuilder(baseUrl).Query = "?" + string.Join("&", refreshParams.Select(x => $"{x.Key}={x.Value}"))}",
            type = "account_onboarding"
        });

        return accountLink.url;
    }

    public dynamic StripeMakeProxyRequest(string endpoint, dynamic payload = null, int version = 1)
    {
        var proxyPayload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "call",
            params = new
            {
                payload,
                proxy_data = StripePrepareProxyData(payload)
            }
        };

        var url = new UriBuilder($"{StripeConstants.ProxyUrl}/{version}/{endpoint}").ToString();

        try
        {
            var response = new HttpClient().PostAsync(url, new StringContent(JsonConvert.SerializeObject(proxyPayload), Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();

            var responseContent = response.Content.ReadAsAsync<dynamic>().Result;
            if (responseContent.error != null)
            {
                var errorData = responseContent.error.data;
                throw new ValidationError($"Stripe Proxy error: {errorData.message}");
            }

            return responseContent.result;
        }
        catch (HttpRequestException e)
        {
            throw new ValidationError("Stripe Proxy: Could not establish the connection.");
        }
        catch (Exception e)
        {
            throw new ValidationError("Stripe Proxy: An error occurred when communicating with the proxy.");
        }
    }

    public dynamic StripePrepareProxyData(dynamic stripePayload = null)
    {
        return new { };
    }

    public string StripeGetPublishableKey()
    {
        return StripeUtils.GetPublishableKey(this.Sudo());
    }

    public string StripeGetInlineFormValues(decimal amount, Currency currency, int partnerId, bool isValidation, PaymentMethod paymentMethodSudo = null)
    {
        var currencyName = isValidation ? this.WithContext(new Dictionary<string, object> { { "validation_pm", paymentMethodSudo } }).GetValidationCurrency().Name.ToLower() : currency.Name.ToLower();
        var partner = Env["res.partner"].WithContext(new Dictionary<string, object> { { "show_address", true } }).Browse(partnerId).Exists();

        return JsonConvert.SerializeObject(new
        {
            publishable_key = StripeGetPublishableKey(),
            currency_name = currencyName,
            minor_amount = amount != 0 ? PaymentUtils.ToMinorCurrencyUnits(amount, currency) : null,
            capture_method = this.CaptureManually ? "manual" : "automatic",
            billing_details = new
            {
                name = partner.Name,
                email = partner.Email,
                phone = partner.Phone,
                address = new
                {
                    line1 = partner.Street,
                    line2 = partner.Street2,
                    city = partner.City,
                    state = partner.State.Code,
                    country = partner.Country.Code,
                    postal_code = partner.Zip
                }
            },
            is_tokenization_required = this.IsTokenizationRequired(),
            payment_methods_mapping = StripeConstants.PaymentMethodsMapping
        });
    }

    public string StripeGetCountry(string countryCode)
    {
        return StripeConstants.CountryMapping.GetValueOrDefault(countryCode, countryCode);
    }

    public string[] GetDefaultPaymentMethodCodes()
    {
        if (this.Code != "stripe")
        {
            return base.GetDefaultPaymentMethodCodes();
        }

        return StripeConstants.DefaultPaymentMethodCodes;
    }
}
