csharp
public partial class ProductTemplate
{
    public string GetProductAccounts()
    {
        var income = PropertyAccountIncomeId ?? CategoryId.PropertyAccountIncomeCategId;
        var expense = PropertyAccountExpenseId ?? CategoryId.PropertyAccountExpenseCategId;
        return $"Income: {income}, Expense: {expense}";
    }

    public Dictionary<string, Account.Account> GetAssetAccounts()
    {
        return new Dictionary<string, Account.Account>
        {
            ["stock_input"] = null,
            ["stock_output"] = null
        };
    }

    public Dictionary<string, Account.Account> GetProductAccounts(Account.FiscalPosition fiscalPos = null)
    {
        var accounts = new Dictionary<string, Account.Account>
        {
            ["income"] = PropertyAccountIncomeId ?? CategoryId.PropertyAccountIncomeCategId,
            ["expense"] = PropertyAccountExpenseId ?? CategoryId.PropertyAccountExpenseCategId
        };

        if (fiscalPos != null)
        {
            accounts["income"] = fiscalPos.MapAccount(accounts["income"]);
            accounts["expense"] = fiscalPos.MapAccount(accounts["expense"]);
        }

        return accounts;
    }

    public void ComputeFiscalCountryCodes()
    {
        var allowedCompanies = CompanyId ?? Env.Companies;
        FiscalCountryCodes = string.Join(",", allowedCompanies.Select(c => c.AccountFiscalCountryId?.Code));
    }

    public void ComputeTaxString()
    {
        TaxString = ConstructTaxString(ListPrice);
    }

    public string ConstructTaxString(decimal price)
    {
        var currency = CurrencyId;
        var res = TaxesId.ComputeAll(price, product: this, partner: Env.Partner);
        var joined = new List<string>();

        var included = res.TotalIncluded;
        if (currency.CompareAmounts(included, price) != 0)
        {
            joined.Add($"{included:C} Incl. Taxes");
        }

        var excluded = res.TotalExcluded;
        if (currency.CompareAmounts(excluded, price) != 0)
        {
            joined.Add($"{excluded:C} Excl. Taxes");
        }

        return joined.Any() ? $"(= {string.Join(", ", joined)})" : " ";
    }
}

public partial class Product
{
    public Dictionary<string, Account.Account> GetProductAccounts()
    {
        return ProductTmplId.GetProductAccounts();
    }

    public decimal GetTaxIncludedUnitPrice(
        Core.Company company, Core.Currency currency, DateTime documentDate, string documentType,
        bool isRefundDocument = false, Core.UoM productUom = null, Core.Currency productCurrency = null,
        decimal? productPriceUnit = null, List<Account.Tax> productTaxes = null, Account.FiscalPosition fiscalPosition = null)
    {
        // Implementation details omitted for brevity
        // This method would need to be adapted to C# conventions and available APIs
        return 0.0m;
    }

    public void ComputeTaxString()
    {
        TaxString = ProductTmplId.ConstructTaxString(LstPrice);
    }

    public Product RetrieveProduct(string name = null, string defaultCode = null, string barcode = null, 
                                   Core.Company company = null, List<(string, string, object)> extraDomain = null)
    {
        // Implementation details omitted for brevity
        // This method would need to be adapted to C# conventions and available APIs
        return null;
    }
}
