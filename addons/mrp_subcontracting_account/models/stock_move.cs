csharp
public partial class MrpSubcontractingAccount.StockMove
{
    public bool ShouldForcePriceUnit()
    {
        if (this.IsSubcontract || base.ShouldForcePriceUnit())
        {
            return true;
        }
        return false;
    }

    public List<Dictionary<string, object>> GenerateValuationLinesData(int partnerId, decimal qty, decimal debitValue, decimal creditValue, int debitAccountId, int creditAccountId, int svlId, string description)
    {
        List<Dictionary<string, object>> rslt = base.GenerateValuationLinesData(partnerId, qty, debitValue, creditValue, debitAccountId, creditAccountId, svlId, description);

        var subcontractProduction = this.ProductionId.Where(p => p.SubcontractorId != null).ToList();
        if (subcontractProduction.Count == 0)
        {
            return rslt;
        }

        // split the credit line to two, one for component cost, one for subcontracting service cost
        decimal componentCost = 0;
        decimal subcontractServiceCost = 0;

        if (this.ProductTmplId.CostMethod == "standard")
        {
            // In case of standard price, the component cost is the cost of the product
            // the subcontracting service cost may not represent the real cost of the subcontracting service
            // the difference should be posted in price difference account in the end
            componentCost = Math.Abs(Env.Company.Currency.Round(subcontractProduction.SelectMany(p => p.MoveRawIds.SelectMany(m => m.StockValuationLayerIds)).Sum(l => l.Value)));
            subcontractServiceCost = creditValue - componentCost;
        }
        else
        {
            subcontractServiceCost = Env.Company.Currency.Round(subcontractProduction.Sum(p => p.ExtraCost * qty));
            componentCost = creditValue - subcontractServiceCost;
        }

        if (!Env.Company.Currency.IsZero(subcontractServiceCost))
        {
            rslt["credit_line_vals"] = null;
            int serviceCostAccount = this.ProductTmplId.GetProductAccounts()["stock_input"].Id;
            rslt["subcontract_credit_line_vals"] = new Dictionary<string, object>()
            {
                { "Name", description },
                { "ProductId", this.ProductTmplId.Id },
                { "Quantity", qty },
                { "ProductUomId", this.ProductUomId.Id },
                { "Ref", description },
                { "PartnerId", partnerId },
                { "Balance", -subcontractServiceCost },
                { "AccountId", serviceCostAccount },
            };
            rslt["component_credit_line_vals"] = new Dictionary<string, object>()
            {
                { "Name", description },
                { "ProductId", this.ProductTmplId.Id },
                { "Quantity", qty },
                { "ProductUomId", this.ProductUomId.Id },
                { "Ref", description },
                { "PartnerId", partnerId },
                { "Balance", -componentCost },
                { "AccountId", creditAccountId },
            };
        }
        // if svl passed is not linked to the move in self, the valuation is a correction and should always credit the
        // `stock_input` account as it adds directly to the value of the subcontracted product
        else if (svlId != 0 && this.StockValuationLayerIds.Count != 0 && !this.StockValuationLayerIds.Any(l => l.Id == svlId))
        {
            rslt["credit_line_vals"]["AccountId"] = this.ProductTmplId.GetProductAccounts()["stock_input"].Id;
        }
        return rslt;
    }

    public int GetDestAccount(Dictionary<string, int> accountData)
    {
        if (this.RawMaterialProductionId.SubcontractorId != null)
        {
            return accountData["production"];
        }
        return base.GetDestAccount(accountData);
    }

    public int GetSrcAccount(Dictionary<string, int> accountData)
    {
        if (this.ProductionId.Any(p => p.SubcontractorId != null))
        {
            return accountData["production"];
        }
        return base.GetSrcAccount(accountData);
    }
}
