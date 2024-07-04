C#
public partial class PaymentProvider {
  public List<Currency> GetSupportedCurrencies() {
    var supportedCurrencies = Env.Call<List<Currency>>("Payment.PaymentProvider", "_GetSupportedCurrencies");
    if (this.Code == "payulatam") {
      supportedCurrencies = supportedCurrencies.Where(c => const.SUPPORTED_CURRENCIES.Contains(c.Name)).ToList();
    }
    return supportedCurrencies;
  }

  public string GenerateSign(Dictionary<string, string> values, bool incoming = true) {
    return Env.Call<string>("Payment.PaymentProvider", "_GenerateSign", new object[] { values, incoming });
  }

  public List<string> GetDefaultPaymentMethodCodes() {
    var defaultCodes = Env.Call<List<string>>("Payment.PaymentProvider", "_GetDefaultPaymentMethodCodes");
    if (this.Code != "payulatam") {
      return defaultCodes;
    }
    return const.DEFAULT_PAYMENT_METHOD_CODES;
  }
}
