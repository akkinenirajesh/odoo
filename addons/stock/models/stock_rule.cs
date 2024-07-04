csharp
public partial class StockRule 
{
    public virtual void CopyData(Dictionary<string, object> defaultValues) 
    {
        // TODO: Implement CopyData
    }

    public virtual void CheckCompanyConsistency() 
    {
        // TODO: Implement CheckCompanyConsistency
    }

    public virtual void OnChangePickingTypeId() 
    {
        // TODO: Implement OnChangePickingTypeId
    }

    public virtual void OnChangeRouteId(Stock.Route routeId, Res.Company companyId) 
    {
        // TODO: Implement OnChangeRouteId
    }

    private Dictionary<string, object> GetMessageValues() 
    {
        // TODO: Implement GetMessageValues
    }

    private Dictionary<string, string> GetMessageDict() 
    {
        // TODO: Implement GetMessageDict
    }

    public virtual void ComputeActionMessage() 
    {
        // TODO: Implement ComputeActionMessage
    }

    public virtual void ComputePickingTypeCodeDomain() 
    {
        // TODO: Implement ComputePickingTypeCodeDomain
    }

    public virtual Stock.StockMove RunPush(Stock.StockMove move) 
    {
        // TODO: Implement RunPush
    }

    private Dictionary<string, object> PushPrepareMoveCopyValues(Stock.StockMove moveToCopy, string newDate) 
    {
        // TODO: Implement PushPrepareMoveCopyValues
    }

    public virtual bool RunPull(List<(Procurement.Procurement, StockRule)> procurements) 
    {
        // TODO: Implement RunPull
    }

    private List<string> GetCustomMoveFields() 
    {
        // TODO: Implement GetCustomMoveFields
    }

    private Dictionary<string, object> GetStockMoveValues(Product.Product productId, decimal productQty, Product.Uom productUom, Stock.Location locationDestId, string name, string origin, Res.Company companyId, Dictionary<string, object> values) 
    {
        // TODO: Implement GetStockMoveValues
    }

    public virtual (Dictionary<string, decimal>, List<(string, string)>) GetLeadDays(Product.Product product, Dictionary<string, object> values) 
    {
        // TODO: Implement GetLeadDays
    }
}
