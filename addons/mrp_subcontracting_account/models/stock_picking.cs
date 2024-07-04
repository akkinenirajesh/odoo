C#
public partial class StockPicking
{
    public StockPicking()
    {
    }

    public ActionReturn ActionViewStockValuationLayers()
    {
        var action = Env.Call<StockPicking>("ActionViewStockValuationLayers", this);
        var subcontractedProductions = GetSubcontractProduction();
        if (subcontractedProductions.Count == 0)
        {
            return action;
        }

        var domain = action.GetProperty<object[]>("domain");
        var domainSubcontracting = new object[] { "id", "in", (subcontractedProductions.GetPropertyValue<object[]>("MoveRawIds") | subcontractedProductions.GetPropertyValue<object[]>("MoveFinishedIds")).GetPropertyValue<object[]>("StockValuationLayerIds").GetPropertyValue<object[]>("ids") };
        domain = new object[] { domain, domainSubcontracting };
        return action.WithProperty("domain", domain);
    }

    private List<Production> GetSubcontractProduction()
    {
        throw new NotImplementedException();
    }
}
