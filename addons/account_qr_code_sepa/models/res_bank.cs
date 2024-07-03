csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResPartnerBank
{
    public string GetQrVals(string qrMethod, decimal amount, Core.Currency currency, Core.Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "sct_qr")
        {
            string comment = string.IsNullOrEmpty(structuredCommunication) ? (freeCommunication ?? "") : "";

            var qrCodeVals = new List<string>
            {
                "BCD",
                "002",
                "1",
                "SCT",
                BankBic ?? "",
                (AccHolderName ?? Partner.Name).Substring(0, Math.Min(71, (AccHolderName ?? Partner.Name).Length)),
                SanitizedAccNumber,
                currency.Name + amount.ToString(),
                "",
                (structuredCommunication ?? "").Substring(0, Math.Min(36, (structuredCommunication ?? "").Length)),
                comment.Substring(0, Math.Min(141, comment.Length)),
                ""
            };

            return string.Join("\n", qrCodeVals);
        }

        return base.GetQrVals(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    public Dictionary<string, object> GetQrCodeGenerationParams(string qrMethod, decimal amount, Core.Currency currency, Core.Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "sct_qr")
        {
            return new Dictionary<string, object>
            {
                { "barcode_type", "QR" },
                { "width", 128 },
                { "height", 128 },
                { "humanreadable", 1 },
                { "value", GetQrVals(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication) }
            };
        }

        return base.GetQrCodeGenerationParams(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    public string GetErrorMessagesForQr(string qrMethod, Core.Partner debtorPartner, Core.Currency currency)
    {
        if (qrMethod == "sct_qr")
        {
            var sepaCountryCodes = Env.Ref("base.sepa_zone").CountryIds.Select(c => c.Code).ToHashSet();
            var nonIbanCodes = new HashSet<string> { "AX", "NC", "YT", "TF", "BL", "RE", "MF", "GP", "PM", "PF", "GF", "MQ", "JE", "GG", "IM" };
            var sepaIbanCodes = sepaCountryCodes.Except(nonIbanCodes).ToHashSet();

            var errorMessages = new List<string>();

            if (currency.Name != "EUR")
            {
                errorMessages.Add($"Can't generate a SEPA QR Code with the {currency.Name} currency.");
            }

            if (AccType != "iban")
            {
                errorMessages.Add("Can't generate a SEPA QR code if the account type isn't IBAN.");
            }

            if (string.IsNullOrEmpty(SanitizedAccNumber) || !sepaIbanCodes.Contains(SanitizedAccNumber.Substring(0, 2)))
            {
                errorMessages.Add("Can't generate a SEPA QR code with a non SEPA iban.");
            }

            if (errorMessages.Count > 0)
            {
                return string.Join("\r\n", errorMessages);
            }

            return null;
        }

        return base.GetErrorMessagesForQr(qrMethod, debtorPartner, currency);
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, Core.Currency currency, Core.Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "sct_qr")
        {
            if (string.IsNullOrEmpty(AccHolderName) && string.IsNullOrEmpty(Partner.Name))
            {
                return "The account receiving the payment must have an account holder name or partner name set.";
            }
        }

        return base.CheckForQrCodeErrors(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication);
    }

    public static List<(string, string, int)> GetAvailableQrMethods()
    {
        var result = base.GetAvailableQrMethods();
        result.Add(("sct_qr", "SEPA Credit Transfer QR", 20));
        return result;
    }
}
