csharp
public partial class ProductProduct
{
    public void ComputeStandardPriceUpdateWarning()
    {
        var undoneExpenses = Env.HrExpense.ReadGroup(
            domain: new[] { 
                ("State", "in", new[] { "draft", "reported" }),
                ("Product", "=", this.Id)
            },
            groupby: new[] { "PriceUnit" }
        );

        var unitAmountsNoWarning = undoneExpenses
            .Select(row => Env.Company.Currency.Round((decimal)row[0]))
            .ToList();
        unitAmountsNoWarning.Add(0.0m); // Add default unit amount

        StandardPriceUpdateWarning = null;
        if (undoneExpenses.Any())
        {
            var roundedPrice = Env.Company.Currency.Round(StandardPrice);
            if (roundedPrice != 0 && (unitAmountsNoWarning.Count > 1 || (unitAmountsNoWarning.Count == 1 && !unitAmountsNoWarning.Contains(roundedPrice))))
            {
                StandardPriceUpdateWarning = "There are unsubmitted expenses linked to this category. Updating the category cost will change expense amounts. Make sure it is what you want to do.";
            }
        }
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        var result = base.Write(vals);

        if (vals.ContainsKey("StandardPrice"))
        {
            var expensesSudo = Env.HrExpense.Sudo().Search(new[]
            {
                ("Company", "=", Env.Company.Id),
                ("Product", "=", this.Id),
                ("State", "in", new[] { "reported", "draft" })
            });

            foreach (var expenseSudo in expensesSudo)
            {
                var expenseProductSudo = expenseSudo.Product;
                var taxDomain = Env.AccountTax.CheckCompanyDomain(expenseSudo.Company);
                var productHasCost = expenseProductSudo != null && !expenseSudo.CompanyCurrency.IsZero(expenseProductSudo.StandardPrice);

                var expenseVals = new Dictionary<string, object>
                {
                    ["ProductHasCost"] = productHasCost,
                    ["ProductHasTax"] = expenseProductSudo?.SupplierTaxes.Any(t => taxDomain(t)) ?? false
                };

                if (productHasCost)
                {
                    expenseVals["PriceUnit"] = expenseProductSudo.StandardPrice;
                }
                else
                {
                    expenseVals["Quantity"] = 1;
                    expenseVals["PriceUnit"] = expenseSudo.TotalAmount;
                }

                expenseSudo.Write(expenseVals);
            }
        }

        return result;
    }
}
