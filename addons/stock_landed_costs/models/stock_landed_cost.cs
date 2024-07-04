C#
public partial class StockLandedCost 
{
    public void ComputeLandedCost()
    {
        // Implementation for ComputeLandedCost
    }

    public void ButtonValidate()
    {
        // Implementation for ButtonValidate
    }

    public void ButtonCancel()
    {
        // Implementation for ButtonCancel
    }

    public void ActionViewStockValuationLayers()
    {
        // Implementation for ActionViewStockValuationLayers
    }

    public List<Stock.Move> GetTargetedMoveIds()
    {
        // Implementation for GetTargetedMoveIds
    }

    public void ReconcileLandedCost()
    {
        // Implementation for ReconcileLandedCost
    }

    public List<Dictionary<string, object>> GetValuationLines()
    {
        // Implementation for GetValuationLines
    }

    public bool CheckCanValidate()
    {
        // Implementation for CheckCanValidate
    }

    public bool CheckSum()
    {
        // Implementation for CheckSum
    }
}

public partial class StockLandedCostLine 
{
    public void OnchangeProductId()
    {
        // Implementation for OnchangeProductId
    }
}

public partial class ValuationAdjustmentLine 
{
    public List<Dictionary<string, object>> CreateAccountingEntries(Account.Move move, double qtyOut)
    {
        // Implementation for CreateAccountingEntries
    }

    public List<Dictionary<string, object>> CreateAccountMoveLine(Account.Move move, int creditAccountId, int debitAccountId, double qtyOut, int alreadyOutAccountId)
    {
        // Implementation for CreateAccountMoveLine
    }
}
