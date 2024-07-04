csharp
public partial class MrpAccount.StockMove {

    public MrpAccount.MrpProduction RawMaterialProductionId { get; set; }

    public MrpAccount.MrpBomLine BomLineId { get; set; }

    public Stock.Location LocationId { get; set; }

    public Stock.Location LocationDestId { get; set; }

    public List<Account.AnalyticDistribution> AnalyticDistribution { get; set; }

    public virtual bool IsProduction() {
        this.EnsureOne();
        return this.LocationId.Usage == "production" && this.LocationDestId.ShouldNotBeValued();
    }

    public virtual bool IsProductionConsumed() {
        this.EnsureOne();
        return this.LocationDestId.Usage == "production" && this.LocationId.ShouldNotBeValued();
    }

    public virtual List<MrpAccount.StockMove> FilterAngloSaxonMoves(Stock.Product product) {
        var res = Env.Call("stock.move", "_filter_anglo_saxon_moves", product);
        res.AddRange(this.Where(m => m.BomLineId.BomId.ProductTmplId.Id == product.ProductTmplId.Id));
        return res;
    }

    public virtual Account.AnalyticDistribution GetAnalyticDistribution() {
        var distribution = this.RawMaterialProductionId.AnalyticDistribution;
        if (distribution != null) {
            return distribution;
        }
        return Env.Call("stock.move", "_get_analytic_distribution");
    }

    public virtual bool ShouldForcePriceUnit() {
        this.EnsureOne();
        return this.PickingTypeId.Code == "mrp_operation" || Env.Call("stock.move", "_should_force_price_unit");
    }

    public virtual bool IgnoreAutomaticValuation() {
        return this.RawMaterialProductionId != null;
    }

    public virtual Account.Account GetSrcAccount(Dictionary<string, Account.Account> accountsData) {
        if (this.IsProduction()) {
            return this.LocationId.ValuationOutAccountId ?? accountsData["production"] ?? accountsData["stock_input"];
        }
        return Env.Call("stock.move", "_get_src_account", accountsData);
    }

    public virtual Account.Account GetDestAccount(Dictionary<string, Account.Account> accountsData) {
        if (this.IsProductionConsumed()) {
            return this.LocationDestId.ValuationInAccountId ?? accountsData["production"] ?? accountsData["stock_output"];
        }
        return Env.Call("stock.move", "_get_dest_account", accountsData);
    }

    private void EnsureOne() {
        if (this.Ids.Count != 1) {
            throw new Exception("This method can only be called on a single record.");
        }
    }
}
