csharp
public partial class ResCountry {
    public bool IsStripeSupportedCountry { get; set; }

    public void ComputeIsStripeSupportedCountry() {
        this.IsStripeSupportedCountry = Env.Stripe.Const.CountryMapping.ContainsKey(this.Code) ? Env.Stripe.Const.CountryMapping[this.Code] : this.Code;
        this.IsStripeSupportedCountry = Env.Stripe.Const.SupportedCountries.Contains(this.IsStripeSupportedCountry);
    }
}
