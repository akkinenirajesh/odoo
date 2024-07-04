csharp
using System;
using System.Text.RegularExpressions;

public partial class ResPartnerBank
{
    public void CheckBrProxy()
    {
        if (CountryCode == "BR" && ProxyType != ProxyType.None)
        {
            if (ProxyType != ProxyType.Email && ProxyType != ProxyType.Mobile && 
                ProxyType != ProxyType.BrCpfCnpj && ProxyType != ProxyType.BrRandom)
            {
                throw new ValidationException("The proxy type must be Email Address, Mobile Number, CPF/CNPJ (BR) or Random Key (BR) for Pix code generation.");
            }

            string value = ProxyValue;
            if (ProxyType == ProxyType.Email && !Env.MailValidation.Validate(value))
            {
                throw new ValidationException($"{value} is not a valid email.");
            }

            if (ProxyType == ProxyType.BrCpfCnpj && 
                (!Partner.CheckVatBr(value) || value.Any(c => !char.IsDigit(c))))
            {
                throw new ValidationException($"{value} is not a valid CPF or CNPJ (don't include periods or dashes).");
            }

            if (ProxyType == ProxyType.Mobile && 
                (string.IsNullOrEmpty(value) || !value.StartsWith("+55") || value.Length != 14))
            {
                throw new ValidationException($"The mobile number {value} is invalid. It must start with +55, contain a 2 digit territory or state code followed by a 9 digit number.");
            }

            string regex = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}";
            if (ProxyType == ProxyType.BrRandom && !Regex.IsMatch(ProxyValue, regex))
            {
                throw new ValidationException($"The random key {value} is invalid, the format looks like this: 71d6c6e1-64ea-4a11-9560-a10870c40ca2");
            }
        }
    }

    public bool ComputeDisplayQrSetting()
    {
        return CountryCode == "BR";
    }

    public string GetAdditionalDataField(string comment)
    {
        if (CountryCode == "BR")
        {
            return Serialize(5, Regex.Replace(comment, @"[^a-zA-Z0-9*]", ""));
        }
        return base.GetAdditionalDataField(comment);
    }

    public List<Tuple<string, string>> GetQrCodeValsList(params object[] args)
    {
        var res = base.GetQrCodeValsList(args);
        if (CountryCode == "BR")
        {
            res[5] = Tuple.Create(res[5].Item1, res[5].Item2 != null ? string.Format("{0:F2}", decimal.Parse(res[5].Item2)) : null);
            res[7] = Tuple.Create(res[7].Item1, res[7].Item2.ToUpper());
            res[8] = Tuple.Create(res[8].Item1, res[8].Item2.ToUpper());
            if (string.IsNullOrEmpty(res[9].Item2))
            {
                res[9] = Tuple.Create(res[9].Item1, GetAdditionalDataField("***"));
            }
        }
        return res;
    }

    public Tuple<int, string> GetMerchantAccountInfo()
    {
        if (CountryCode == "BR")
        {
            var merchantAccountInfoData = new[]
            {
                Tuple.Create(0, "br.gov.bcb.pix"),
                Tuple.Create(1, ProxyValue)
            };
            return Tuple.Create(26, string.Join("", merchantAccountInfoData.Select(val => Serialize(val.Item1, val.Item2))));
        }
        return base.GetMerchantAccountInfo();
    }

    public string GetErrorMessagesForQr(string qrMethod, Partner debtorPartner, Currency currency)
    {
        if (qrMethod == "emv_qr" && CountryCode == "BR")
        {
            if (currency.Name != "BRL")
            {
                return "Can't generate a Pix QR code with a currency other than BRL.";
            }
            return null;
        }
        return base.GetErrorMessagesForQr(qrMethod, debtorPartner, currency);
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, Currency currency, Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "emv_qr" && CountryCode == "BR" &&
            ProxyType != ProxyType.Email && ProxyType != ProxyType.Mobile &&
            ProxyType != ProxyType.BrCpfCnpj && ProxyType != ProxyType.BrRandom)
        {
            return $"To generate a Pix code the proxy type for {this} must be Email Address, Mobile Number, CPF/CNPJ (BR) or Random Key (BR).";
        }
        return base.CheckForQrCodeErrors(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }
}
