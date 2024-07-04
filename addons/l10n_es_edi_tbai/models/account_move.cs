csharp
public partial class AccountMove
{
    public override string ToString()
    {
        return Name;
    }

    public bool ComputeL10nEsTbaiIsRequired()
    {
        return (IsSaleDocument() || (IsPurchaseDocument() && Company.L10nEsTbaiTaxAgency == "bizkaia"
                && !InvoiceLineIds.Any(l => l.TaxIds.Any(t => t.L10nEsType == "ignore")))
            && CountryCode == "ES"
            && Company.L10nEsTbaiTaxAgency != null;
    }

    public bool ComputeShowResetToDraftButton()
    {
        if (L10nEsTbaiChainIndex != 0)
        {
            return false;
        }
        // Call base implementation
        return base.ComputeShowResetToDraftButton();
    }

    public void ButtonDraft()
    {
        if (L10nEsTbaiChainIndex != 0 && EdiState != "cancelled")
        {
            throw new UserException("You cannot reset to draft an entry that has been posted to TicketBAI's chain");
        }
        // Call base implementation
        base.ButtonDraft();
    }

    public void OnDeleteL10nEsTbaiUnlinkExceptInChain()
    {
        if (!Env.Context.ContainsKey("force_delete") && L10nEsTbaiChainIndex != 0)
        {
            throw new UserException("You cannot delete a move that has a TicketBAI chain id.");
        }
    }

    public bool L10nEsTbaiIsInChain()
    {
        var tbaiDocIds = EdiDocumentIds.Where(d => d.EdiFormatId.Code == "es_tbai");
        return L10nEsTbaiIsRequired
            && tbaiDocIds.Any()
            && !tbaiDocIds.Any(d => d.State == "to_send");
    }

    public (string Sequence, string Number) GetL10nEsTbaiSequenceAndNumber()
    {
        var parts = Name.Split('/');
        var sequence = string.Join("/", parts.Take(parts.Length - 1));
        var number = parts.Last();

        sequence = System.Text.RegularExpressions.Regex.Replace(sequence, @"[^0-9A-Za-z.\_\-\/]", "");
        sequence = System.Text.RegularExpressions.Regex.Replace(sequence, @"\s+", " ");

        if (Company.L10nEsEdiTestEnv)
        {
            sequence += "TEST";
        }

        return (sequence, number);
    }

    // Implement other methods similarly...
}
