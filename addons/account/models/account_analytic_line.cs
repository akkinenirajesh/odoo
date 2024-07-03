csharp
public partial class AccountAnalyticLine
{
    public void ComputeGeneralAccountId()
    {
        GeneralAccountId = MoveLineId?.AccountId;
    }

    public void CheckGeneralAccountId()
    {
        if (MoveLineId != null && GeneralAccountId != MoveLineId.AccountId)
        {
            throw new ValidationException("The journal item is not linked to the correct financial account");
        }
    }

    public void ComputePartnerId()
    {
        PartnerId = MoveLineId?.PartnerId ?? PartnerId;
    }

    public void OnChangeUnitAmount()
    {
        if (ProductId == null)
        {
            return;
        }

        var prodAccounts = ProductId.ProductTemplateId.WithCompany(CompanyId).GetProductAccounts();
        var unit = ProductUomId;
        var account = prodAccounts["expense"];
        if (unit == null || ProductId.UomPoId.CategoryId.Id != unit.CategoryId.Id)
        {
            unit = ProductId.UomPoId;
        }

        // Compute based on pricetype
        var amountUnit = ProductId.PriceCompute("standard_price", uom: unit)[ProductId.Id];
        var amount = amountUnit * UnitAmount ?? 0.0;
        var result = (CurrencyId?.Round(amount) ?? Math.Round(amount, 2)) * -1;
        Amount = result;
        GeneralAccountId = account;
        ProductUomId = unit;
    }

    public string ViewHeaderGet(int viewId, string viewType)
    {
        if (Env.Context.TryGetValue("account_id", out var accountId))
        {
            var account = Env.Get<AccountAnalyticAccount>().Browse((int)accountId);
            return $"Entries: {account.Name}";
        }
        return base.ViewHeaderGet(viewId, viewType);
    }
}
