csharp
public partial class AccountTax
{
    public HashSet<int> HookComputeIsUsed(HashSet<int> taxesToCompute)
    {
        var usedTaxes = base.HookComputeIsUsed(taxesToCompute);
        taxesToCompute.ExceptWith(usedTaxes);

        if (taxesToCompute.Count > 0)
        {
            Env.GetModel<HrExpense>().FlushModel(new[] { "TaxIds" });
            
            var query = @"
                SELECT id
                FROM account_tax
                WHERE EXISTS(
                    SELECT 1
                    FROM expense_tax AS exp
                    WHERE tax_id IN @TaxIds
                    AND account_tax.id = exp.tax_id
                )";

            var parameters = new { TaxIds = taxesToCompute };
            var result = Env.Cr.Query<int>(query, parameters);

            usedTaxes.UnionWith(result);
        }

        return usedTaxes;
    }
}
