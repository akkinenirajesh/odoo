C#
public partial class SaleOrderTemplate
{
    public bool Active { get; set; }

    public ResCompany Company { get; set; }

    public string Name { get; set; }

    public string Note { get; set; }

    public int Sequence { get; set; }

    public List<MailTemplate> MailTemplate { get; set; }

    public int NumberOfDays { get; set; }

    public bool RequireSignature { get; set; }

    public bool RequirePayment { get; set; }

    public double PrepaymentPercent { get; set; }

    public List<SaleOrderTemplateLine> SaleOrderTemplateLine { get; set; }

    public List<SaleOrderTemplateOption> SaleOrderTemplateOption { get; set; }

    public AccountJournal Journal { get; set; }

    public void ComputeRequireSignature()
    {
        RequireSignature = (Company ?? Env.Company).PortalConfirmationSign;
    }

    public void ComputeRequirePayment()
    {
        RequirePayment = (Company ?? Env.Company).PortalConfirmationPay;
    }

    public void ComputePrepaymentPercent()
    {
        PrepaymentPercent = (Company ?? Env.Company).PrepaymentPercent;
    }

    public void OnChangePrepaymentPercent()
    {
        if (PrepaymentPercent == 0)
        {
            RequirePayment = false;
        }
    }

    public void CheckCompany()
    {
        var companies = SaleOrderTemplateLine.Select(x => x.Product.Company).Union(SaleOrderTemplateOption.Select(x => x.Product.Company)).ToList();

        if (companies.Count > 1)
        {
            throw new Exception("Your template cannot contain products from multiple companies.");
        }

        if (companies.Any() && !companies.Contains(Company))
        {
            throw new Exception($"Your template contains products from company {string.Join(", ", companies.Select(x => x.DisplayName))} whereas your template belongs to company {Company.DisplayName}. \n Please change the company of your template or remove the products from other companies.");
        }
    }

    public void CheckPrepaymentPercent()
    {
        if (RequirePayment && !(0 < PrepaymentPercent && PrepaymentPercent <= 1.0))
        {
            throw new Exception("Prepayment percentage must be a valid percentage.");
        }
    }

    public void Create(List<SaleOrderTemplate> valsList)
    {
        var records = (List<SaleOrderTemplate>)Env.Create(valsList);
        UpdateProductTranslations(records);
    }

    public void Write(List<SaleOrderTemplate> vals)
    {
        if (vals.Contains("Active") && !vals.Get("Active"))
        {
            var companies = Env.Search<ResCompany>().Where(x => x.SaleOrderTemplate == this).ToList();
            companies.ForEach(x => x.SaleOrderTemplate = null);
        }

        Env.Write(vals);
        UpdateProductTranslations(this);
    }

    private void UpdateProductTranslations(List<SaleOrderTemplate> records)
    {
        var languages = Env.Search<ResLang>().Where(x => x.Active).ToList();

        foreach (var lang in languages)
        {
            foreach (var line in records.SelectMany(x => x.SaleOrderTemplateLine))
            {
                if (line.Name == line.Product.GetProductMultilineDescriptionSale(lang.Code))
                {
                    line.Name = line.Product.GetProductMultilineDescriptionSale(lang.Code);
                }
            }

            foreach (var option in records.SelectMany(x => x.SaleOrderTemplateOption))
            {
                if (option.Name == option.Product.GetProductMultilineDescriptionSale(lang.Code))
                {
                    option.Name = option.Product.GetProductMultilineDescriptionSale(lang.Code);
                }
            }
        }
    }
}
