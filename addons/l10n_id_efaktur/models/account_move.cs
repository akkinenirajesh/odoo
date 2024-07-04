csharp
public partial class AccountMove
{
    public override string ToString()
    {
        return Name;
    }

    public void OnChangeL10nIdTaxNumber()
    {
        if (L10nIdTaxNumber != null && !GetPurchaseTypes().Contains(MoveType))
        {
            throw new UserException("You can only change the number manually for a Vendor Bills and Credit Notes");
        }
    }

    public void ComputeCsvCreated()
    {
        L10nIdCsvCreated = L10nIdAttachmentId != null;
    }

    public void ComputeKodeTransaksi()
    {
        L10nIdKodeTransaksi = Partner.CommercialPartnerId.L10nIdKodeTransaksi;
    }

    public void ComputeNeedKodeTransaksi()
    {
        L10nIdNeedKodeTransaksi = Partner.CommercialPartnerId.L10nIdPkp
            && L10nIdTaxNumber == null
            && MoveType == "out_invoice"
            && CountryCode == "ID"
            && LineIds.Any(l => l.TaxIds.Any());
    }

    public void ConstraintKodePpn()
    {
        var ppnTag = Env.Ref("l10n_id.ppn_tag");
        
        if (L10nIdNeedKodeTransaksi && L10nIdKodeTransaksi != "08")
        {
            if (LineIds.Where(l => l.DisplayType == "product").Any(l => l.TaxTagIds.Contains(ppnTag.Id))
                && LineIds.Where(l => l.DisplayType == "product").Any(l => !l.TaxTagIds.Contains(ppnTag.Id)))
            {
                throw new UserException("Cannot mix VAT subject and Non-VAT subject items in the same invoice with this kode transaksi.");
            }
        }

        if (L10nIdNeedKodeTransaksi && L10nIdKodeTransaksi == "08")
        {
            if (LineIds.Where(l => l.DisplayType == "product").Any(l => l.TaxTagIds.Contains(ppnTag.Id)))
            {
                throw new UserException("Kode transaksi 08 is only for non VAT subject items.");
            }
        }
    }

    public void ConstrainsL10nIdTaxNumber()
    {
        if (L10nIdTaxNumber != null)
        {
            L10nIdTaxNumber = new string(L10nIdTaxNumber.Where(char.IsDigit).ToArray());
            
            if (L10nIdTaxNumber.Length != 16)
            {
                throw new UserException("A tax number should have 16 digits");
            }
            else if (!Env.Get<AccountMove>().Fields.L10nIdKodeTransaksi.Selection.Keys.Contains(L10nIdTaxNumber.Substring(0, 2)))
            {
                throw new UserException("A tax number must begin by a valid Kode Transaksi");
            }
            else if (L10nIdTaxNumber[2] != '0' && L10nIdTaxNumber[2] != '1')
            {
                throw new UserException("The third digit of a tax number must be 0 or 1");
            }
        }
    }

    // Additional methods would be implemented here...
}
