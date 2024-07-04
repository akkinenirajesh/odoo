csharp
public partial class PosPaymentMethod {

    public bool IsOnlinePayment { get; set; }
    public ICollection<PaymentProvider> OnlinePaymentProviderIds { get; set; }
    public bool HasAnOnlinePaymentProvider { get; set; }
    public PosPaymentMethodType Type { get; set; }

    public void ComputeHasAnOnlinePaymentProvider()
    {
        if (IsOnlinePayment)
        {
            HasAnOnlinePaymentProvider = GetOnlinePaymentProviders().Any();
        }
        else
        {
            HasAnOnlinePaymentProvider = false;
        }
    }

    private IEnumerable<PaymentProvider> GetOnlinePaymentProviders(int posConfigId = 0, bool errorIfInvalid = true)
    {
        var providersSudo = OnlinePaymentProviderIds;
        if (!providersSudo.Any()) // Empty = all published providers
        {
            providersSudo = Env.Search<PaymentProvider>(x => x.IsPublished == true && x.State == "enabled" || x.State == "test");
        }

        if (posConfigId == 0)
        {
            return providersSudo;
        }

        var configCurrency = Env.Search<PosConfig>(x => x.Id == posConfigId).FirstOrDefault().CurrencyId;
        var validProviders = providersSudo.Where(p => p.JournalId.CurrencyId == null || p.JournalId.CurrencyId == configCurrency);
        if (errorIfInvalid && providersSudo.Count != validProviders.Count())
        {
            throw new Exception("All payment providers configured for an online payment method must use the same currency as the Sales Journal, or the company currency if that is not set, of the POS config.");
        }
        return validProviders;
    }
    
    public void _LoadPosDataFields(int configId)
    {
        var paramsList = new List<string>();
        paramsList.AddRange(base._LoadPosDataFields(configId));
        paramsList.Add("IsOnlinePayment");
    }

    public void _ComputeType()
    {
        if (IsOnlinePayment)
        {
            Type = PosPaymentMethodType.online;
        }
        else
        {
            base._ComputeType();
        }
    }

    // ... Other methods and properties
}
