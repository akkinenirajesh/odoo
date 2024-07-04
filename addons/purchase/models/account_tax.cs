C#
public partial class AccountTax
{
    public bool IsUsed { get; set; }
    public List<PurchaseOrderLine> PurchaseOrderLines { get; set; }


    public List<AccountTax> HookComputeIsUsed(List<AccountTax> taxesToCompute)
    {
        List<AccountTax> usedTaxes = Env.CallMethod<List<AccountTax>>(this, "HookComputeIsUsed", taxesToCompute);
        taxesToCompute = taxesToCompute.Except(usedTaxes).ToList();

        if (taxesToCompute.Count > 0)
        {
            Env.FlushModel<PurchaseOrderLine>(new List<string> { "Taxes" });
            var ids = Env.ExecuteQuery<int>(
                """
                SELECT id
                FROM account_tax
                WHERE EXISTS(
                    SELECT 1
                    FROM account_tax_purchase_order_line_rel AS pur
                    WHERE account_tax_id IN @taxesToCompute
                    AND account_tax.id = pur.account_tax_id
                )
                """,
                new { taxesToCompute = taxesToCompute.Select(t => t.Id).ToList() }
            );
            usedTaxes.AddRange(ids.Select(id => new AccountTax() { Id = id }));
        }
        return usedTaxes;
    }
}
