csharp
public partial class AccountMove
{
    public bool ComputeL10nChQrIsValid()
    {
        var errorMessages = this.PartnerBankId.GetErrorMessagesForQr("ch_qr", this.PartnerId, this.CurrencyId);
        return this.MoveType == "out_invoice" && !errorMessages.Any();
    }

    public string GetL10nChQrrNumber()
    {
        if (this.PartnerBankId.L10nChQrIban && this.L10nChIsQrValid && !string.IsNullOrEmpty(this.Name))
        {
            string invoiceRef = System.Text.RegularExpressions.Regex.Replace(this.Name, @"[^\d]", "");
            return ComputeQrrNumber(invoiceRef);
        }
        return null;
    }

    public string ComputeQrrNumber(string invoiceRef)
    {
        const int L10N_CH_QRR_NUMBER_LENGTH = 27;
        int refPayloadLen = L10N_CH_QRR_NUMBER_LENGTH - 1;
        int extra = invoiceRef.Length - refPayloadLen;
        if (extra > 0)
        {
            invoiceRef = invoiceRef.Substring(extra);
        }
        string internalRef = invoiceRef.PadLeft(refPayloadLen, '0');
        return Env.Tools.Mod10r(internalRef);
    }

    public string GetInvoiceReferenceCHInvoice()
    {
        return GetL10nChQrrNumber();
    }

    public string GetInvoiceReferenceCHPartner()
    {
        return GetL10nChQrrNumber();
    }

    public string SpaceQrrReference(string qrrRef)
    {
        string spacedQrrRef = "";
        int i = qrrRef.Length;
        while (i > 0)
        {
            spacedQrrRef = qrrRef.Substring(Math.Max(i - 5, 0), Math.Min(5, i)) + " " + spacedQrrRef;
            i -= 5;
        }
        return spacedQrrRef.Trim();
    }

    public string SpaceScorReference(string iso11649Ref)
    {
        return string.Join(" ", Enumerable.Range(0, iso11649Ref.Length / 4)
            .Select(i => iso11649Ref.Substring(i * 4, 4)));
    }

    public object L10nChActionPrintQr()
    {
        if (this.MoveType != "out_invoice")
        {
            throw new UserException("Only customers invoices can be QR-printed.");
        }
        if (!this.L10nChIsQrValid)
        {
            return new
            {
                name = "Some invoices could not be printed in the QR format",
                type = "ir.actions.act_window",
                res_model = "l10n_ch.qr_invoice.wizard",
                view_mode = "form",
                target = "new",
                context = new { active_ids = new[] { this.Id } }
            };
        }
        return Env.Ref("account.account_invoices").ReportAction(this);
    }

    public Dictionary<string, List<AccountMove>> L10nChDispatchInvoicesToPrint()
    {
        return new Dictionary<string, List<AccountMove>>
        {
            { "qr", new List<AccountMove> { this }.Where(x => x.L10nChIsQrValid).ToList() },
            { "classic", new List<AccountMove> { this }.Where(x => !x.L10nChIsQrValid).ToList() }
        };
    }
}
