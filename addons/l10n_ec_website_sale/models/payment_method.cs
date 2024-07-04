csharp
public partial class PaymentMethod
{
    public string _GetFiscalCountryCodes()
    {
        return string.Join(",", Env.Companies.Select(c => c.AccountFiscalCountry?.Code).Where(c => c != null));
    }

    public override string ToString()
    {
        // Implement a meaningful string representation of PaymentMethod
        return $"Payment Method: {L10nEcSriPayment?.Name ?? "N/A"}";
    }
}
