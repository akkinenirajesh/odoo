csharp
public partial class ProductTemplate
{
    public override string ToString()
    {
        // Implement string representation logic
        return Name; // Assuming there's a Name field in the actual model
    }

    public object DefaultGet(string[] fields)
    {
        var result = base.DefaultGet(fields);
        if (Env.Context.GetValueOrDefault("default_can_be_expensed", false))
        {
            result["SupplierTaxesId"] = null;
        }
        return result;
    }

    private void _ComputeCanBeExpensed()
    {
        if (Type != ProductType.Consu && Type != ProductType.Service || !PurchaseOk)
        {
            CanBeExpensed = false;
        }
    }

    private void _ComputePurchaseOk()
    {
        if (CanBeExpensed)
        {
            PurchaseOk = true;
        }
    }

    public static void AutoInit()
    {
        // This method would need to be called during application startup or database initialization
        if (!Env.Cr.ColumnExists("product_template", "can_be_expensed"))
        {
            Env.Cr.CreateColumn("product_template", "can_be_expensed", "boolean");
            Env.Cr.Execute(@"
                UPDATE product_template
                SET can_be_expensed = false
                WHERE type NOT IN ('consu', 'service')
            ");
        }
    }
}
