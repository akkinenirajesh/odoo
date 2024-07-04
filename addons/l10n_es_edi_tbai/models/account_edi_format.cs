csharp
public partial class AccountEdiFormat
{
    public override string ToString()
    {
        return Name;
    }

    public bool NeedsWebServices()
    {
        return Code == "es_tbai" || base.NeedsWebServices();
    }

    public bool IsEnabledByDefaultOnJournal(Journal journal)
    {
        if (Code != "es_sii")
        {
            return base.IsEnabledByDefaultOnJournal(journal);
        }
        return false;
    }

    public bool IsCompatibleWithJournal(Journal journal)
    {
        if (Code != "es_tbai")
        {
            return base.IsCompatibleWithJournal(journal);
        }

        return journal.CountryCode == "ES" && (journal.Type == "sale" || journal.Type == "purchase");
    }

    public Dictionary<string, object> GetMoveApplicability(AccountMove move)
    {
        if (Code != "es_tbai" || move.CountryCode != "ES" || !move.L10nEsTbaiIsRequired)
        {
            return base.GetMoveApplicability(move);
        }

        return new Dictionary<string, object>
        {
            { "post", L10nEsTbaiPostInvoiceEdi },
            { "cancel", L10nEsTbaiCancelInvoiceEdi },
            { "edi_content", L10nEsTbaiGetInvoiceContentEdi }
        };
    }

    public List<string> CheckMoveConfiguration(AccountMove invoice)
    {
        var errors = base.CheckMoveConfiguration(invoice);

        if (Code != "es_tbai" || invoice.CountryCode != "ES")
        {
            return errors;
        }

        if (!invoice.Company.L10nEsEdiCertificateId)
        {
            errors.Add("Please configure the certificate for TicketBAI/SII.");
        }

        if (!invoice.Company.L10nEsTbaiTaxAgency.Any())
        {
            errors.Add("Please specify a tax agency on your company for TicketBAI.");
        }

        if (string.IsNullOrEmpty(invoice.Company.Vat))
        {
            errors.Add("Please configure the Tax ID on your company for TicketBAI.");
        }

        if (invoice.MoveType == "out_refund")
        {
            if (string.IsNullOrEmpty(invoice.L10nEsTbaiRefundReason))
            {
                throw new ValidationException("Refund reason must be specified (TicketBAI)");
            }
            if (invoice.L10nEsIsSimplified)
            {
                if (invoice.L10nEsTbaiRefundReason != "R5")
                {
                    throw new ValidationException("Refund reason must be R5 for simplified invoices (TicketBAI)");
                }
            }
            else
            {
                if (invoice.L10nEsTbaiRefundReason == "R5")
                {
                    throw new ValidationException("Refund reason cannot be R5 for non-simplified invoices (TicketBAI)");
                }
            }
        }

        return errors;
    }

    // Additional methods would be implemented here...
}
