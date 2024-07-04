csharp
public partial class AccountMove
{
    public string L10nKeFmt(string str, int length, bool ljust = true)
    {
        if (string.IsNullOrEmpty(str))
            str = "";
        
        var cleaned = System.Text.RegularExpressions.Regex.Replace(str, "[^A-Za-z0-9 ]+", "");
        var encoded = System.Text.Encoding.GetEncoding(1251).GetBytes(cleaned);
        
        if (ljust)
            Array.Resize(ref encoded, length);
        else
            encoded = encoded.Take(length).ToArray();
        
        return System.Text.Encoding.GetEncoding(1251).GetString(encoded);
    }

    public List<(string, List<string>)> L10nKeValidateMove()
    {
        var errors = new List<(string, List<string>)>();
        var moveErrors = new List<string>();

        if (this.CountryCode != "KE")
            moveErrors.Add("This invoice is not a Kenyan invoice and therefore can not be sent to the device.");

        if (this.Company.Currency.Id != Env.Ref("base.KES").Id)
            moveErrors.Add("This invoice's company currency is not in Kenyan Shillings, conversion to KES is not possible.");

        if (this.State != "posted")
            moveErrors.Add("This invoice/credit note has not been posted. Please confirm it to continue.");

        if (this.MoveType != "out_refund" && this.MoveType != "out_invoice")
            moveErrors.Add("The document being sent should be an invoice or credit note.");

        if (this.L10nKeCuInvoiceNumber != null || this.L10nKeCuSerialNumber != null || 
            this.L10nKeCuQrcode != null || this.L10nKeCuDateTime != null)
            moveErrors.Add("The document already has details related to the fiscal device. Please make sure that the invoice has not already been sent.");

        if (this.MoveType == "out_refund" && this.ReversedEntry.L10nKeCuInvoiceNumber == null)
            moveErrors.Add("This credit note must reference the previous invoice, and this previous invoice must have already been submitted.");

        // Add more validation logic here...

        if (moveErrors.Any())
            errors.Add((this.Name, moveErrors));

        return errors;
    }

    public bool L10nKeFiscalDeviceDetailsFilled()
    {
        return this.CountryCode == "KE" &&
               !string.IsNullOrEmpty(this.L10nKeCuInvoiceNumber) &&
               !string.IsNullOrEmpty(this.L10nKeCuSerialNumber) &&
               !string.IsNullOrEmpty(this.L10nKeCuQrcode) &&
               this.L10nKeCuDateTime.HasValue;
    }

    public List<byte[]> L10nKeGetCuMessages()
    {
        var msgs = new List<byte[]>();
        msgs.AddRange(L10nKeCuOpenInvoiceMessage());
        msgs.AddRange(L10nKeCuLinesMessages());
        msgs.Add(Encoding.ASCII.GetBytes("\x38"));
        msgs.Add(Encoding.ASCII.GetBytes("\x68"));
        return msgs;
    }

    public Dictionary<string, object> L10nKeActionCuPost()
    {
        var errors = L10nKeValidateMove();
        if (errors.Any())
        {
            var errorMsg = string.Join("\n\n", errors.Select(e => 
                $"Invalid invoice configuration on {e.Item1}:\n{string.Join("\n", e.Item2)}"));
            throw new UserError(errorMsg);
        }

        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.client",
            ["tag"] = "l10n_ke_post_send",
            ["params"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["move_id"] = this.Id,
                    ["messages"] = JsonConvert.SerializeObject(L10nKeGetCuMessages().Select(m => Encoding.GetEncoding(1251).GetString(m))),
                    ["proxy_address"] = this.Company.L10nKeCuProxyAddress,
                    ["company_vat"] = this.Company.Vat,
                    ["name"] = this.Name
                }
            }
        };
    }

    public void L10nKeCuResponses(List<Dictionary<string, object>> responses)
    {
        foreach (var response in responses)
        {
            var moveId = Convert.ToInt32(response["move_id"]);
            var move = Env["Account.AccountMove"].Browse(moveId);
            var replies = ((List<object>)response["replies"]).Cast<string>().ToList();

            move.Write(new Dictionary<string, object>
            {
                ["L10nKeCuSerialNumber"] = response["serial_number"],
                ["L10nKeCuInvoiceNumber"] = replies[replies.Count - 2].Split(';')[0],
                ["L10nKeCuQrcode"] = replies[replies.Count - 2].Split(';')[1].Trim(),
                ["L10nKeCuDateTime"] = DateTime.ParseExact(replies[replies.Count - 1], "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture)
            });
        }
    }

    // Implement other methods like L10nKeCuOpenInvoiceMessage, L10nKeCuLinesMessages, etc.
}
