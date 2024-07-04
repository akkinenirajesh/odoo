csharp
public partial class PosRestaurantAdyen.PosPaymentMethod
{
    public virtual string AdyenMerchantAccount { get; set; }

    public virtual Dictionary<string, string> GetAdyenEndpoints()
    {
        var endpoints = Env.Model("PosRestaurantAdyen.PosPaymentMethod").Call<Dictionary<string, string>>("_GetAdyenEndpoints");
        endpoints["Adjust"] = $"https://pal-{Env.Context.Company.AdyenMerchantAccount}.adyen.com/pal/servlet/Payment/v52/adjustAuthorisation";
        endpoints["Capture"] = $"https://pal-{Env.Context.Company.AdyenMerchantAccount}.adyen.com/pal/servlet/Payment/v52/capture";
        return endpoints;
    }

    public virtual List<string> LoadPosDataFields(int configId)
    {
        var params = Env.Model("PosRestaurantAdyen.PosPaymentMethod").Call<List<string>>("_LoadPosDataFields", configId);
        params.Add("AdyenMerchantAccount");
        return params;
    }
}
