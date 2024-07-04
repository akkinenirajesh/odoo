csharp
using System;
using System.Text.RegularExpressions;

public partial class ResPartnerBank
{
    private static readonly Regex AutoMobnRegex = new Regex(@"^[+]\d{1,3}-\d{6,12}$");
    private static readonly Regex SingleEmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

    public void CheckHkProxy()
    {
        if (CountryCode == "HK")
        {
            if (ProxyType != ProxyType.Id && ProxyType != ProxyType.Mobile && ProxyType != ProxyType.Email)
            {
                throw new ValidationException($"The FPS Type must be either ID, Mobile or Email to generate a FPS QR code for account number {AccNumber}.");
            }

            if (ProxyType == ProxyType.Id && (string.IsNullOrEmpty(ProxyValue) || (ProxyValue.Length != 7 && ProxyValue.Length != 9)))
            {
                throw new ValidationException($"Invalid FPS ID! Please enter a valid FPS ID with length 7 or 9 for account number {AccNumber}.");
            }

            if (ProxyType == ProxyType.Mobile && (string.IsNullOrEmpty(ProxyValue) || !AutoMobnRegex.IsMatch(ProxyValue)))
            {
                throw new ValidationException($"Invalid Mobile! Please enter a valid mobile number with format +852-67891234 for account number {AccNumber}.");
            }

            if (ProxyType == ProxyType.Email && (string.IsNullOrEmpty(ProxyValue) || !SingleEmailRegex.IsMatch(ProxyValue)))
            {
                throw new ValidationException($"Invalid Email! Please enter a valid email address for account number {AccNumber}.");
            }
        }
    }

    public void ComputeDisplayQrSetting()
    {
        if (CountryCode == "HK")
        {
            DisplayQrSetting = true;
        }
        else
        {
            // Call base implementation for other countries
            base.ComputeDisplayQrSetting();
        }
    }

    public Tuple<int, string> GetMerchantAccountInfo()
    {
        if (CountryCode == "HK")
        {
            int fpsType = ProxyType switch
            {
                ProxyType.Id => 2,
                ProxyType.Mobile => 3,
                ProxyType.Email => 4,
                _ => throw new InvalidOperationException("Invalid ProxyType")
            };

            var merchantAccountVals = new[]
            {
                Tuple.Create(0, "hk.com.hkicl"),
                Tuple.Create(fpsType, ProxyValue)
            };

            string merchantAccountInfo = string.Join("", merchantAccountVals.Select(val => Serialize(val.Item1, val.Item2)));
            return Tuple.Create(26, merchantAccountInfo);
        }
        
        return base.GetMerchantAccountInfo();
    }

    public string GetAdditionalDataField(string comment)
    {
        if (CountryCode == "HK")
        {
            return Serialize(5, comment);
        }
        
        return base.GetAdditionalDataField(comment);
    }

    public string GetErrorMessagesForQr(string qrMethod, Partner debtorPartner, Currency currency)
    {
        if (qrMethod == "emv_qr" && CountryCode == "HK")
        {
            if (currency.Name != "HKD" && currency.Name != "CNY")
            {
                return "Can't generate a FPS QR code with a currency other than HKD or CNY.";
            }
            return null;
        }

        return base.GetErrorMessagesForQr(qrMethod, debtorPartner, currency);
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, Currency currency, Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "emv_qr" && CountryCode == "HK" && ProxyType != ProxyType.Id && ProxyType != ProxyType.Mobile && ProxyType != ProxyType.Email)
        {
            return "The FPS Type must be either ID, Mobile or Email to generate a FPS QR code.";
        }

        return base.CheckForQrCodeErrors(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    private string Serialize(int id, string value)
    {
        // Implementation of serialization logic
        throw new NotImplementedException();
    }
}
