csharp
public partial class WebsitePaymentProvider
{
    public WebsitePaymentProvider(Env env)
    {
        Env = env;
    }

    public Env Env { get; }

    public IQueryable<PaymentProvider> GetCompatibleProviders(int websiteId, Dictionary<string, object> kwargs)
    {
        var providers = Env.Call("payment.provider", "_get_compatible_providers", kwargs: kwargs);
        if (websiteId != 0)
        {
            var unfilteredProviders = providers;
            providers = providers.Where(p => p.Website == null || p.Website.Id == websiteId);
            Env.Call("payment", "add_to_report", args: new object[]
            {
                kwargs["report"],
                unfilteredProviders.Except(providers),
                false,
                Env.GetConstant("payment", "REPORT_REASONS_MAPPING")["incompatible_website"]
            });
        }
        return providers;
    }

    public string GetBaseUrl()
    {
        if (Env.Context.Get("request") != null)
        {
            var urlRoot = (string)Env.Context.Get("request").Get("httprequest").Get("url_root");
            if (!string.IsNullOrEmpty(urlRoot))
            {
                return IriToUri(urlRoot);
            }
        }
        return Env.Call("payment.provider", "get_base_url");
    }

    public PaymentProvider Copy(Dictionary<string, object> defaultValues)
    {
        var res = Env.Call<PaymentProvider>("payment.provider", "copy", defaultValues);
        if (Env.Context.Get("stripe_connect_onboarding") != null)
        {
            res.Website = null;
        }
        return res;
    }

    private string IriToUri(string iri)
    {
        // Implement logic to convert IRI to URI using your preferred method
        // You can use a library like System.Uri or implement your own logic
        // This example uses a simple string replacement for demonstration
        return iri.Replace(":", "%3A");
    }
}
