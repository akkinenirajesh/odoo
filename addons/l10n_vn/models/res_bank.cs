csharp
public partial class l10n_vn.ResPartnerBank {
    public void CheckVnProxy() {
        if (this.CountryCode == "VN" && this.ProxyType != "merchant_id" && this.ProxyType != "payment_service" && this.ProxyType != "atm_card" && this.ProxyType != "bank_acc" && this.ProxyType != "none" && this.ProxyType != null) {
            throw new ValidationError(string.Format("The QR Code Type must be either Merchant ID, ATM Card Number or Bank Account to generate a Vietnam Bank QR code for account number {0}.", this.AccNumber));
        }
    }

    public void ComputeDisplayQrSetting() {
        if (this.CountryCode == "VN") {
            this.DisplayQrSetting = true;
        } else {
            // Call base method for other countries.
            // Not possible to implement without access to other countries' logic.
            // Assuming base class exists with a method named ComputeDisplayQrSetting()
            // this.ComputeDisplayQrSetting(); 
        }
    }

    public List<Tuple<int, string>> GetMerchantAccountInfo() {
        if (this.CountryCode == "VN") {
            var proxyTypeMapping = new Dictionary<string, string>()
            {
                { "merchant_id", "QRPUSH" },
                { "payment_service", "QRPUSH" },
                { "atm_card", "QRIBFTTC" },
                { "bank_acc", "QRIBFTTA" },
            };
            var paymentNetwork = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(0, this.BankBic),
                new Tuple<int, string>(1, this.ProxyValue)
            };
            var vals = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(0, "A000000727"),
                new Tuple<int, string>(1, string.Concat(paymentNetwork.Select(val => this.Serialize(val.Item1, val.Item2)))),
                new Tuple<int, string>(2, proxyTypeMapping[this.ProxyType])
            };
            return new List<Tuple<int, string>>() { new Tuple<int, string>(38, string.Concat(vals.Select(val => this.Serialize(val.Item1, val.Item2)))) };
        } else {
            // Call base method for other countries.
            // Not possible to implement without access to other countries' logic.
            // Assuming base class exists with a method named GetMerchantAccountInfo()
            // return this.GetMerchantAccountInfo();
        }
        return null;
    }

    public string GetAdditionalDataField(string comment) {
        if (this.CountryCode == "VN") {
            return this.Serialize(8, comment);
        } else {
            // Call base method for other countries.
            // Not possible to implement without access to other countries' logic.
            // Assuming base class exists with a method named GetAdditionalDataField()
            // return this.GetAdditionalDataField(comment);
        }
        return null;
    }

    public string GetErrorMessagesForQr(string qrMethod, ResPartner debtorPartner, Currency currency) {
        if (qrMethod == "emv_qr" && this.CountryCode == "VN") {
            if (currency.Name != "VND") {
                return "Can't generate a Vietnamese QR banking code with a currency other than VND.";
            }
            if (this.BankBic == null) {
                return "Missing Bank Identifier Code.\nPlease configure the Bank Identifier Code inside the bank settings.";
            }
        } else {
            // Call base method for other countries.
            // Not possible to implement without access to other countries' logic.
            // Assuming base class exists with a method named GetErrorMessagesForQr()
            // return this.GetErrorMessagesForQr(qrMethod, debtorPartner, currency);
        }
        return null;
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, Currency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication) {
        if (qrMethod == "emv_qr" && this.CountryCode == "VN" && this.ProxyType != "merchant_id" && this.ProxyType != "payment_service" && this.ProxyType != "atm_card" && this.ProxyType != "bank_acc") {
            return string.Format("The proxy type {0} is not supported for Vietnamese partners. It must be either Merchant ID, ATM Card Number or Bank Account", this.ProxyType);
        } else {
            // Call base method for other countries.
            // Not possible to implement without access to other countries' logic.
            // Assuming base class exists with a method named CheckForQrCodeErrors()
            // return this.CheckForQrCodeErrors(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
        }
        return null;
    }

    private string Serialize(int code, string value) {
        return string.Format("{0}{1}", code.ToString().PadLeft(3, '0'), value);
    }
}
