csharp
public partial class AccountMove
{
    public string GetNameInvoiceReport()
    {
        if (Company.Country.IsIn(Env.Ref<Core.CountryGroup>("base.gulf_cooperation_council").Countries))
        {
            return "l10n_gcc_invoice.arabic_english_invoice";
        }
        return base.GetNameInvoiceReport();
    }

    public string Num2Words(decimal number, string lang)
    {
        try
        {
            return Num2Words.Convert(number, lang).ToTitleCase();
        }
        catch (Exception)
        {
            Env.Logger.Warning("The library 'num2words' is missing, cannot render textual amounts.");
            return "";
        }
    }

    public void LoadNarrationTranslation()
    {
        if (this == null)
            return;

        var movesToFix = new List<AccountMove>();
        foreach (var move in this.Where(m => 
            !string.IsNullOrEmpty(m.Narration) && 
            m.IsSaleDocument(includeReceipts: true) && 
            m.Company.Country.IsIn(Env.Ref<Core.CountryGroup>("base.gulf_cooperation_council").Countries)))
        {
            string lang = move.Partner.Lang ?? Env.User.Lang;
            if (move.Company.TermsType == "html" || move.Narration != move.Company.WithContext(lang).InvoiceTerms)
                continue;
            movesToFix.Add(move);
        }

        if (!movesToFix.Any())
            return;

        var companyIds = movesToFix.Select(m => m.Company.Id).Distinct().ToList();
        var translations = Env.Cr.Query<int, string>(
            "SELECT \"id\", \"invoice_terms\" FROM \"res_company\" WHERE id = ANY(@ids)",
            new { ids = companyIds }
        ).ToDictionary(r => r.Item1, r => r.Item2);

        foreach (var move in movesToFix)
        {
            move.Narration = translations[move.Company.Id];
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();
        LoadNarrationTranslation();
    }

    public override void ComputeNarration()
    {
        base.ComputeNarration();
        if (Id != 0)
            LoadNarrationTranslation();
    }
}

public partial class AccountMoveLine
{
    public void ComputeTaxAmount()
    {
        L10nGccInvoiceTaxAmount = PriceTotal - PriceSubtotal;
    }

    public void ComputeL10nGccLineName()
    {
        string LangProductName(string lang) => 
            WithContext(new { lang }).Product.DisplayName;

        if (Product != null && new[] { LangProductName("ar_001"), LangProductName("en_US") }.Contains(Name))
        {
            L10nGccLineName = LangProductName(Move.Partner.Lang);
        }
        else
        {
            L10nGccLineName = Name;
        }
    }
}
