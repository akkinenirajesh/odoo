csharp
public partial class AccountMoveLine
{
    public override void OnValidate()
    {
        base.OnValidate();
        if (Move?.ExpenseSheetId?.PaymentMode != "company_account")
        {
            CheckPayableReceivable();
        }
    }

    public override List<AttachmentDomain> GetAttachmentDomains()
    {
        var attachmentDomains = base.GetAttachmentDomains();
        if (ExpenseId != null)
        {
            attachmentDomains.Add(new AttachmentDomain
            {
                ResModel = "HR.Expense",
                ResId = ExpenseId.Id
            });
        }
        return attachmentDomains;
    }

    public override void ComputeTaxKey()
    {
        base.ComputeTaxKey();
        if (ExpenseId != null)
        {
            TaxKey = new Dictionary<string, object>(TaxKey)
            {
                { "expense_id", ExpenseId.Id }
            };
        }
    }

    public override void ComputeAllTax()
    {
        if (ExpenseId != null)
        {
            using (new EnvironmentContext(Env, new Dictionary<string, object> { { "force_price_include", true } }))
            {
                base.ComputeAllTax();
            }
        }
        else
        {
            base.ComputeAllTax();
        }

        if (ExpenseId != null)
        {
            var updatedComputeAllTax = new Dictionary<Dictionary<string, object>, object>();
            foreach (var entry in ComputeAllTax)
            {
                var newKey = new Dictionary<string, object>(entry.Key)
                {
                    { "expense_id", ExpenseId.Id }
                };
                updatedComputeAllTax[newKey] = entry.Value;
            }
            ComputeAllTax = updatedComputeAllTax;
        }
    }

    public override void ComputeTotals()
    {
        if (ExpenseId != null)
        {
            using (new EnvironmentContext(Env, new Dictionary<string, object> { { "force_price_include", true } }))
            {
                base.ComputeTotals();
            }
        }
        else
        {
            base.ComputeTotals();
        }
    }

    public override Dictionary<string, object> ConvertToTaxBaseLineDict()
    {
        var result = base.ConvertToTaxBaseLineDict();
        if (ExpenseId != null)
        {
            if (!result.ContainsKey("extra_context"))
            {
                result["extra_context"] = new Dictionary<string, object>();
            }
            ((Dictionary<string, object>)result["extra_context"])["force_price_include"] = true;
        }
        return result;
    }

    public override string GetExtraQueryBaseTaxLineMapping()
    {
        return " AND (base_line.ExpenseId IS NULL OR account_move_line.ExpenseId = base_line.ExpenseId)";
    }
}
