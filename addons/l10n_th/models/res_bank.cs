csharp
public partial class ResPartnerBank {
    public void CheckThProxy() {
        var taxIdRe = new System.Text.RegularExpressions.Regex(r"^[0-9]{13}$");
        var mobileRe = new System.Text.RegularExpressions.Regex(r"^[0-9]{10}$");
        if (this.ProxyType != ResPartnerBankProxyType.EwalletId &&
            this.ProxyType != ResPartnerBankProxyType.MerchantTaxId &&
            this.ProxyType != ResPartnerBankProxyType.Mobile &&
            this.ProxyType != ResPartnerBankProxyType.None) {
            throw new Exception($"The QR Code Type must be either Ewallet ID, Merchant Tax ID or Mobile Number to generate a Thailand Bank QR code for account number {this.AccNumber}");
        }
        if (this.ProxyType == ResPartnerBankProxyType.MerchantTaxId && (string.IsNullOrEmpty(this.ProxyValue) || !taxIdRe.IsMatch(this.ProxyValue))) {
            throw new Exception($"The Merchant Tax ID must be in the format 1234567890123 for account number {this.AccNumber}");
        }
        if (this.ProxyType == ResPartnerBankProxyType.Mobile && (string.IsNullOrEmpty(this.ProxyValue) || !mobileRe.IsMatch(this.ProxyValue))) {
            throw new Exception($"The Mobile Number must be in the format 0812345678 for account number {this.AccNumber}");
        }
    }

    public void ComputeDisplayQrSetting() {
        if (this.CountryCode == "TH") {
            this.DisplayQrSetting = true;
        }
    }

    public string GetMerchantAccountInfo() {
        if (this.CountryCode == "TH") {
            var proxyTypeMapping = new Dictionary<ResPartnerBankProxyType, int> {
                { ResPartnerBankProxyType.Mobile, 1 },
                { ResPartnerBankProxyType.MerchantTaxId, 2 },
                { ResPartnerBankProxyType.EwalletId, 3 },
            };
            var proxyValue = this.ProxyType == ResPartnerBankProxyType.Mobile ? "66" + this.ProxyValue.PadLeft(13, '0') : this.ProxyValue;
            var vals = new List<Tuple<int, string>> {
                new Tuple<int, string>(0, "A000000677010111"),
                new Tuple<int, string>(proxyTypeMapping[this.ProxyType], proxyValue),
            };
            return string.Concat(vals.Select(val => Serialize(val.Item1, val.Item2)));
        }
        return Env.Call("ResPartnerBank", "_get_merchant_account_info", this);
    }

    public string GetErrorMessagesForQr(string qrMethod, ResPartner debtorPartner, CoreCurrency currency) {
        if (qrMethod == "emv_qr" && this.CountryCode == "TH") {
            if (currency.Name != "THB") {
                return "Can't generate a PayNow QR code with a currency other than THB.";
            }
            return null;
        }
        return Env.Call("ResPartnerBank", "_get_error_messages_for_qr", this, qrMethod, debtorPartner, currency);
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, CoreCurrency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication) {
        if (qrMethod == "emv_qr" && this.CountryCode == "TH" && this.ProxyType != ResPartnerBankProxyType.EwalletId && this.ProxyType != ResPartnerBankProxyType.MerchantTaxId && this.ProxyType != ResPartnerBankProxyType.Mobile) {
            return "The PayNow Type must be either Ewallet ID, Merchant Tax ID or Mobile Number to generate a Thailand Bank QR code";
        }
        return Env.Call("ResPartnerBank", "_check_for_qr_code_errors", this, qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    private string Serialize(int key, string value) {
        return $"{key.ToString().PadLeft(11, '0')}{value}";
    }
}
