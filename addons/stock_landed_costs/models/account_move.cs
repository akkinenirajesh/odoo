csharp
public partial class AccountMove {
    public bool LandedCostsVisible { get; set; }

    public IEnumerable<StockLandedCost> LandedCostsIds { get; set; }

    public void ComputeLandedCostsVisible()
    {
        if (LandedCostsIds != null && LandedCostsIds.Any())
        {
            LandedCostsVisible = false;
        }
        else
        {
            LandedCostsVisible = Env.GetRecordset<AccountMoveLine>().Where(line => line.IsLandedCostsLine).Any();
        }
    }

    public void ButtonCreateLandedCosts()
    {
        var landedCostsLines = Env.GetRecordset<AccountMoveLine>().Where(line => line.IsLandedCostsLine);
        
        var landedCosts = Env.CreateRecord<StockLandedCost>(new {
            VendorBillId = this.Id,
            CostLines = landedCostsLines.Select(l => new {
                ProductId = l.ProductId,
                Name = l.ProductId.Name,
                AccountId = l.ProductId.ProductTemplateId.GetProductAccounts().StockInput.Id,
                PriceUnit = l.CurrencyId.Convert(l.PriceSubtotal, l.CompanyCurrencyId, l.CompanyId, l.MoveId.Date),
                SplitMethod = l.ProductId.SplitMethodLandedCost ?? "equal"
            })
        });
        
        var action = Env.GetAction("stock_landed_costs.action_stock_landed_cost");
        var dict = new { 
            action.Id,
            view_mode = "form",
            res_id = landedCosts.Id,
            views = new List<(int, string)> { (0, "form") }
        };
    }

    public void ActionViewLandedCosts()
    {
        var action = Env.GetAction("stock_landed_costs.action_stock_landed_cost");
        var domain = new List<object> { ("id", "in", LandedCostsIds.Select(x => x.Id).ToList()) };
        var context = new Dictionary<string, object> { { "default_vendor_bill_id", this.Id } };
        var views = new List<(int, string)> { (Env.GetView("stock_landed_costs.view_stock_landed_cost_tree2").Id, "tree"), (0, "form"), (0, "kanban") };
        var dict = new { 
            action.Id,
            domain,
            context,
            views
        };
    }

    public void Post(bool soft = true)
    {
        var posted = base.Post(soft);
        posted.LandedCostsIds.ForEach(x => x.ReconcileLandedCost());
    }
}

public partial class AccountMoveLine {
    public Selection ProductType { get; set; }
    public bool IsLandedCostsLine { get; set; }

    public void OnchangeProductId()
    {
        if (ProductId.LandedCostOk)
        {
            IsLandedCostsLine = true;
        }
        else
        {
            IsLandedCostsLine = false;
        }
    }

    public void OnchangeIsLandedCostsLine()
    {
        if (IsLandedCostsLine && ProductId != null && ProductType != "service")
        {
            IsLandedCostsLine = false;
        }
    }

    public IEnumerable<StockValuationLayer> GetStockValuationLayers(AccountMove move)
    {
        var layers = base.GetStockValuationLayers(move);
        return layers.Where(svl => svl.StockLandedCostId == null);
    }

    public bool EligibleForCogs()
    {
        return base.EligibleForCogs() || (ProductId.Type == "service" && ProductId.LandedCostOk);
    }
}
