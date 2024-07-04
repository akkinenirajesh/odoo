csharp
public partial class PosOrder
{
    public string GetNewHash(int secureSeqNumber)
    {
        // Get the only one exact previous order in the securisation sequence
        var prevOrder = Env.Query<PosOrder>()
            .Where(o => new[] { "paid", "done", "invoiced" }.Contains(o.State)
                && o.Company == this.Company
                && o.L10nFrSecureSequenceNumber != 0
                && o.L10nFrSecureSequenceNumber == secureSeqNumber - 1)
            .FirstOrDefault();

        if (prevOrder == null)
        {
            throw new UserException("An error occurred when computing the inalterability. Impossible to get the unique previous posted point of sale order.");
        }

        // Build and return the hash
        var computedHash = ComputeHash(prevOrder?.L10nFrHash ?? "");
        Env.Logger.Info(
            $"Computed hash for order ID {this.Id}: {computedHash}\nString to hash: {this.L10nFrStringToHash}\nPrevious hash: {prevOrder?.L10nFrHash}");
        return computedHash;
    }

    private string ComputeHash(string previousHash)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(previousHash + this.L10nFrStringToHash));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public void ComputeStringToHash()
    {
        var values = new Dictionary<string, object>
        {
            ["date_order"] = this.DateOrder,
            ["user_id"] = this.User.Id,
            ["lines"] = this.Lines.OrderBy(l => l.Id).Select(l => l.Id).ToList(),
            ["payment_ids"] = this.Payments.OrderBy(p => p.Id).Select(p => p.Id).ToList(),
            ["pricelist_id"] = this.Pricelist?.Id,
            ["partner_id"] = this.Partner?.Id,
            ["session_id"] = this.Session?.Id,
            ["pos_reference"] = this.PosReference,
            ["sale_journal"] = this.SaleJournal?.Id,
            ["fiscal_position_id"] = this.FiscalPosition?.Id
        };

        foreach (var line in this.Lines)
        {
            values[$"line_{line.Id}_notice"] = line.Notice;
            values[$"line_{line.Id}_product_id"] = line.Product?.Id;
            values[$"line_{line.Id}_qty"] = line.Qty;
            values[$"line_{line.Id}_price_unit"] = line.PriceUnit;
            values[$"line_{line.Id}_discount"] = line.Discount;
            values[$"line_{line.Id}_tax_ids"] = line.Taxes.OrderBy(t => t.Id).Select(t => t.Id).ToList();
            values[$"line_{line.Id}_tax_ids_after_fiscal_position"] = line.TaxesAfterFiscalPosition.OrderBy(t => t.Id).Select(t => t.Id).ToList();
        }

        this.L10nFrStringToHash = System.Text.Json.JsonSerializer.Serialize(values, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public override bool Write(IDictionary<string, object> vals)
    {
        var hasBeenPosted = false;

        if (this.Company.IsAccountingUnalterable())
        {
            // Write the hash and the secure_sequence_number when posting or invoicing a pos.order
            if (vals.TryGetValue("State", out var stateObj) && stateObj is string state &&
                (state == "paid" || state == "done" || state == "invoiced"))
            {
                hasBeenPosted = true;
            }

            // Restrict the operation in case we are trying to write a forbidden field
            if ((this.State == "paid" || this.State == "done" || this.State == "invoiced") &&
                vals.Keys.Intersect(new[] { "DateOrder", "User", "Lines", "Payments", "Pricelist", "Partner", "Session", "PosReference", "SaleJournal", "FiscalPosition" }).Any())
            {
                throw new UserException("According to the French law, you cannot modify a point of sale order. Forbidden fields: " +
                    string.Join(", ", new[] { "DateOrder", "User", "Lines", "Payments", "Pricelist", "Partner", "Session", "PosReference", "SaleJournal", "FiscalPosition" }));
            }

            // Restrict the operation in case we are trying to overwrite existing hash
            if ((this.L10nFrHash != null && vals.ContainsKey("L10nFrHash")) ||
                (this.L10nFrSecureSequenceNumber != 0 && vals.ContainsKey("L10nFrSecureSequenceNumber")))
            {
                throw new UserException("You cannot overwrite the values ensuring the inalterability of the point of sale.");
            }
        }

        var result = base.Write(vals);

        // Write the hash and the secure_sequence_number when posting or invoicing a pos order
        if (hasBeenPosted && this.Company.IsAccountingUnalterable() &&
            (this.L10nFrSecureSequenceNumber == 0 || string.IsNullOrEmpty(this.L10nFrHash)))
        {
            var newNumber = this.Company.L10nFrPosCertSequence.NextByCompany(this.Company);
            var valsHashing = new Dictionary<string, object>
            {
                ["L10nFrSecureSequenceNumber"] = newNumber,
                ["L10nFrHash"] = this.GetNewHash(newNumber)
            };
            result &= base.Write(valsHashing);
        }

        return result;
    }

    public override bool Unlink()
    {
        if (this.Company.IsAccountingUnalterable())
        {
            throw new UserException("According to French law, you cannot delete a point of sale order.");
        }
        return base.Unlink();
    }
}
