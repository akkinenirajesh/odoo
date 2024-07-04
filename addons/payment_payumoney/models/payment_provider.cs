csharp
public partial class PaymentProvider {
    public string _get_supported_currencies() {
        var supportedCurrencies = Env.Call("payment.provider", "_get_supported_currencies");
        if (this.Code == "payumoney") {
            supportedCurrencies = Env.Call("payment.provider", "_get_supported_currencies", this).Where(c => c.Name == "INR").ToList();
        }
        return supportedCurrencies;
    }

    public string _payumoney_generate_sign(Dictionary<string, object> values, bool incoming) {
        var signValues = new Dictionary<string, object>() {
            { "key", this.PayUmoneyMerchantKey },
            { "salt", this.PayUmoneyMerchantSalt },
        };
        signValues = signValues.Union(values).ToDictionary(x => x.Key, x => x.Value);
        string sign;
        if (incoming) {
            string[] keys = new string[] { "salt", "status", "", "", "", "", "", "", "udf5", "udf4", "udf3", "udf2", "udf1", "email", "firstname", "productinfo", "amount", "txnid", "key" };
            sign = string.Join("|", keys.Select(k => signValues.GetValueOrDefault(k) ?? "").ToArray());
        } else { // outgoing
            string[] keys = new string[] { "key", "txnid", "amount", "productinfo", "firstname", "email", "udf1", "udf2", "udf3", "udf4", "udf5", "", "", "", "", "salt" };
            sign = string.Join("|", keys.Select(k => signValues.GetValueOrDefault(k) ?? "").ToArray());
        }
        return System.Security.Cryptography.SHA512.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(sign)).Aggregate("", (s, b) => s + b.ToString("x2"));
    }

    public List<string> _get_default_payment_method_codes() {
        var defaultCodes = Env.Call("payment.provider", "_get_default_payment_method_codes");
        if (this.Code != "payumoney") {
            return defaultCodes;
        }
        return DEFAULT_PAYMENT_METHOD_CODES;
    }
}
