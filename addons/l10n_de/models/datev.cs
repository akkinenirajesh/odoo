csharp
public partial class ProductTemplate
{
    public Dictionary<string, Account.Account> GetProductAccounts()
    {
        var result = base.GetProductAccounts();
        var company = Env.Company;

        if (company.AccountFiscalCountry?.Code == "DE")
        {
            if (PropertyAccountIncome == null)
            {
                var taxes = Taxes.Where(t => t.Company == company);
                if (result["income"] == null || (result["income"].TaxIds.Any() && taxes.Any() && !result["income"].TaxIds.Contains(taxes.First())))
                {
                    result["income"] = Env.Set<Account.Account>()
                        .Search(new[]
                        {
                            new object[] { "company", "=", company.Id },
                            new object[] { "internal_group", "=", "income" },
                            new object[] { "deprecated", "=", false },
                            new object[] { "tax_ids", "in", taxes.Select(t => t.Id).ToArray() }
                        })
                        .FirstOrDefault();
                }
            }

            if (PropertyAccountExpense == null)
            {
                var supplierTaxes = SupplierTaxes.Where(t => t.Company == company);
                if (result["expense"] == null || (result["expense"].TaxIds.Any() && supplierTaxes.Any() && !result["expense"].TaxIds.Contains(supplierTaxes.First())))
                {
                    result["expense"] = Env.Set<Account.Account>()
                        .Search(new[]
                        {
                            new object[] { "company", "=", company.Id },
                            new object[] { "internal_group", "=", "expense" },
                            new object[] { "deprecated", "=", false },
                            new object[] { "tax_ids", "in", supplierTaxes.Select(t => t.Id).ToArray() }
                        })
                        .FirstOrDefault();
                }
            }
        }

        return result;
    }
}
