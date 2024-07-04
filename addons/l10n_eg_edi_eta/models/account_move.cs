csharp
public partial class AccountMove
{
    public string ComputeEtaLongId()
    {
        var responseData = L10nEgEtaJsonDocId?.Raw != null 
            ? JsonConvert.DeserializeObject<dynamic>(L10nEgEtaJsonDocId.Raw)?.response 
            : null;
        return responseData?.l10n_eg_long_id ?? null;
    }

    public string ComputeEtaQrCodeStr()
    {
        if (InvoiceDate != null && !string.IsNullOrEmpty(L10nEgUuid) && !string.IsNullOrEmpty(L10nEgLongId))
        {
            var isProd = Company.L10nEgProductionEnv;
            var baseUrl = Env.Get<AccountEdiFormat>().L10nEgGetEtaQrDomain(isProd);
            return $"{baseUrl}/documents/{L10nEgUuid}/share/{L10nEgLongId}";
        }
        return string.Empty;
    }

    public void ComputeEtaResponseData()
    {
        var responseData = L10nEgEtaJsonDocId?.Raw != null 
            ? JsonConvert.DeserializeObject<dynamic>(L10nEgEtaJsonDocId.Raw)?.response 
            : null;
        if (responseData != null)
        {
            L10nEgUuid = responseData.l10n_eg_uuid;
            L10nEgSubmissionNumber = responseData.l10n_eg_submission_number;
            L10nEgLongId = responseData.l10n_eg_long_id;
        }
        else
        {
            L10nEgUuid = null;
            L10nEgSubmissionNumber = null;
            L10nEgLongId = null;
        }
    }

    public void ButtonDraft()
    {
        L10nEgEtaJsonDocId = null;
        L10nEgIsSigned = false;
        // Call base implementation
        base.ButtonDraft();
    }

    public void ActionPostSignInvoices()
    {
        var invoices = Env.Get<AccountMove>().Search(x => 
            x.CountryCode == "EG" && 
            x.State == "posted" && 
            string.IsNullOrEmpty(x.L10nEgSubmissionNumber) &&
            x.EdiDocumentIds.Any(e => e.EdiFormatId.Code == "eg_eta"));

        if (!invoices.Any())
        {
            return;
        }

        var companyIds = invoices.Select(x => x.Company).Distinct().ToList();
        if (companyIds.Count > 1)
        {
            throw new UserError("Please only sign invoices from one company at a time");
        }

        var companyId = companyIds.First();
        var driveId = Env.Get<L10nEgEdiThumbDrive>().Search(x => 
            x.User == Env.User && 
            x.Company == companyId).FirstOrDefault();

        if (driveId == null)
        {
            throw new ValidationError($"Please setup a personal drive for company {companyId.Name}");
        }

        if (string.IsNullOrEmpty(driveId.Certificate))
        {
            throw new ValidationError("Please setup the certificate on the thumb drive menu");
        }

        foreach (var invoice in invoices)
        {
            invoice.L10nEgSigningTime = DateTime.UtcNow;
            var etaInvoice = Env.Get<AccountEdiFormat>().L10nEgEtaPrepareEtaInvoice(invoice);
            var attachment = Env.Get<IrAttachment>().Create(new IrAttachment
            {
                Name = $"ETA_INVOICE_DOC_{invoice.Name}",
                ResId = invoice.Id,
                ResModel = invoice.GetType().Name,
                Type = "binary",
                Raw = JsonConvert.SerializeObject(new { request = etaInvoice }),
                Mimetype = "application/json",
                Description = $"Egyptian Tax authority JSON invoice generated for {invoice.Name}."
            });
            invoice.L10nEgEtaJsonDocId = attachment;
        }

        return driveId.ActionSignInvoices(this);
    }

    public void ActionGetEtaInvoicePdf()
    {
        var etaInvoicePdf = Env.Get<AccountEdiFormat>().L10nEgGetEtaInvoicePdf(this);
        if (etaInvoicePdf.ContainsKey("error"))
        {
            // Log warning
            return;
        }
        
        this.WithContext(new { no_new_invoice = true })
            .MessagePost(body: "ETA invoice has been received", 
                         attachments: new[] { ($"ETA invoice of {Name}.pdf", etaInvoicePdf["data"]) });
    }

    public decimal L10nEgEdiExchangeCurrencyRate()
    {
        var fromCurrency = Currency;
        var toCurrency = Company.Currency;
        if (fromCurrency != toCurrency && InvoiceLineIds.Any())
        {
            var amountCurrency = InvoiceLineIds.First().AmountCurrency;
            if (!FromCurrency.IsZero(amountCurrency))
            {
                return Math.Abs(InvoiceLineIds.First().Balance / amountCurrency);
            }
        }
        return 1.0m;
    }
}
