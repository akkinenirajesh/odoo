csharp
public partial class ResPartnerBank
{
    public string ComputeL10nChQrIban()
    {
        try
        {
            ValidateQrIban(this.AccNumber);
            return this.SanitizedAccNumber;
        }
        catch (ValidationException)
        {
            return null;
        }
    }

    public bool ComputeL10nChDisplayQrBankOptions()
    {
        if (this.Partner != null)
        {
            return this.Partner.RefCompanyIds.Any(c => c.Country?.Code == "CH" || c.Country?.Code == "LI");
        }
        else if (this.Company != null)
        {
            return this.Company.AccountFiscalCountry?.Code == "CH" || this.Company.AccountFiscalCountry?.Code == "LI";
        }
        else
        {
            return Env.Company.AccountFiscalCountry?.Code == "CH" || Env.Company.AccountFiscalCountry?.Code == "LI";
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (!string.IsNullOrEmpty(this.L10nChQrIban))
        {
            ValidateQrIban(this.L10nChQrIban);
            this.L10nChQrIban = PrettyIban(NormalizeIban(this.L10nChQrIban));
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (!string.IsNullOrEmpty(this.L10nChQrIban))
        {
            ValidateQrIban(this.L10nChQrIban);
            this.L10nChQrIban = PrettyIban(NormalizeIban(this.L10nChQrIban));
        }
    }

    public List<string> L10nChGetQrVals(decimal amount, Currency currency, Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        // Implementation of _l10n_ch_get_qr_vals method
        // This method would return a list of strings as per the original Python implementation
        // You'll need to implement the logic here, adapting it to C# and your system's structure
    }

    public Dictionary<string, object> GetQrCodeGenerationParams(string qrMethod, decimal amount, Currency currency, Partner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "ch_qr")
        {
            return new Dictionary<string, object>
            {
                ["barcode_type"] = "QR",
                ["width"] = 256,
                ["height"] = 256,
                ["quiet"] = 1,
                ["mask"] = "ch_cross",
                ["value"] = string.Join("\n", L10nChGetQrVals(amount, currency, debtorPartner, freeCommunication, structuredCommunication)),
                ["barLevel"] = "M",
            };
        }
        // Implement base method call or other logic for different qr methods
        return null;
    }

    // Implement other methods like _get_partner_address_lines, _is_qr_reference, _is_iso11649_reference, etc.
}
