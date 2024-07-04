csharp
public partial class PosConfig
{
    public bool IsSpanish
    {
        get => _ComputeIsSpanish();
        set { /* setter logic if needed */ }
    }

    public Account.Journal SimplifiedInvoiceJournal
    {
        get => _DefaultSinvJournalId();
        set { /* setter logic if needed */ }
    }

    public Core.Partner SimplifiedPartner
    {
        get => _ComputeSimplifiedPartnerId();
        set { /* setter logic if needed */ }
    }

    private bool _ComputeIsSpanish()
    {
        return this.Company.CountryCode == "ES";
    }

    private Account.Journal _DefaultSinvJournalId()
    {
        return Env.AccountJournal.Search(new[]
        {
            Env.AccountJournal.CheckCompanyDomain(Env.Company),
            ("Type", "=", "Sale"),
            ("Code", "=", "SINV")
        }).FirstOrDefault();
    }

    private Core.Partner _ComputeSimplifiedPartnerId()
    {
        return Env.Ref("l10n_es.partner_simplified");
    }

    public List<int> GetLimitedPartnersLoading()
    {
        var res = base.GetLimitedPartnersLoading();
        if (!res.Contains(this.SimplifiedPartner.Id))
        {
            res.Add(this.SimplifiedPartner.Id);
        }
        return res;
    }

    public void EnsureSinvJournal()
    {
        var company = this.Company;
        if (company.ChartTemplate.StartsWith("es_"))
        {
            var sinvJournal = _DefaultSinvJournalId();
            if (sinvJournal == null)
            {
                var incomeAccount = Env.Ref($"account.{company.Id}_account_common_7000", false);
                sinvJournal = Env.AccountJournal.Create(new Dictionary<string, object>
                {
                    ["Type"] = "Sale",
                    ["Name"] = "Simplified Invoices",
                    ["Code"] = "SINV",
                    ["DefaultAccount"] = incomeAccount?.Id,
                    ["Company"] = company.Id,
                    ["Sequence"] = 30
                });
            }
            if (this.SimplifiedInvoiceJournal == null)
            {
                this.SimplifiedInvoiceJournal = sinvJournal;
            }
        }
    }
}
