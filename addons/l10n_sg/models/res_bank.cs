csharp
public partial class ResPartnerBank
{
    public void CheckSgProxy()
    {
        if (this.CountryCode == "SG" && this.ProxyType != "Mobile" && this.ProxyType != "Uen" && this.ProxyType != "None")
        {
            throw new Exception($"The PayNow Type must be either Mobile or UEN to generate a PayNow QR code for account number {this.AccNumber}.");
        }
    }

    public void ComputeDisplayQRSetting()
    {
        if (this.CountryCode == "SG")
        {
            this.DisplayQRSetting = true;
        }
    }

    public Tuple<int, string> GetMerchantAccountInfo()
    {
        if (this.CountryCode == "SG")
        {
            Dictionary<string, int> proxyTypeMapping = new Dictionary<string, int>()
            {
                { "Mobile", 0 },
                { "Uen", 2 },
            };

            List<Tuple<int, object>> merchantAccountVals = new List<Tuple<int, object>>()
            {
                new Tuple<int, object>(0, "SG.PAYNOW"),
                new Tuple<int, object>(1, proxyTypeMapping[this.ProxyType]),
                new Tuple<int, object>(2, this.ProxyValue),
                new Tuple<int, object>(3, 0),
            };

            string merchantAccountInfo = string.Join("", merchantAccountVals.Select(x => Serialize(x.Item1, x.Item2)));
            return new Tuple<int, string>(26, merchantAccountInfo);
        }

        return Env.CallMethod<ResPartnerBank, Tuple<int, string>>(this, "GetMerchantAccountInfo");
    }

    public object GetAdditionalDataField(string comment)
    {
        if (this.CountryCode == "SG")
        {
            return Serialize(1, comment);
        }

        return Env.CallMethod<ResPartnerBank, object>(this, "GetAdditionalDataField", comment);
    }

    public string GetErrorMessagesForQR(string qrMethod, ResPartner debtorPartner, Currency currency)
    {
        if (qrMethod == "emv_qr" && this.CountryCode == "SG")
        {
            if (currency.Name != "SGD")
            {
                return "Can't generate a PayNow QR code with a currency other than SGD.";
            }

            return null;
        }

        return Env.CallMethod<ResPartnerBank, string>(this, "GetErrorMessagesForQR", qrMethod, debtorPartner, currency);
    }

    public string CheckForQRcodeErrors(string qrMethod, decimal amount, Currency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "emv_qr" && this.CountryCode == "SG" && this.ProxyType != "Mobile" && this.ProxyType != "Uen")
        {
            return "The PayNow Type must be either Mobile Number or UEN.";
        }

        return Env.CallMethod<ResPartnerBank, string>(this, "CheckForQRcodeErrors", qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    private string Serialize(int code, object value)
    {
        // Implementation for serialization based on your specific needs
        // Example:
        return $"{(char)code}{value}";
    }
}
