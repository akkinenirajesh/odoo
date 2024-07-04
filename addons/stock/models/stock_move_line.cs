C#
public partial class StockMoveLine
{
    public StockMoveLine()
    {
    }
    
    public void ComputeProductUomId()
    {
        // TODO: Implement ComputeProductUomId
    }

    public void ComputeLotsVisible()
    {
        // TODO: Implement ComputeLotsVisible
    }

    public void ComputePicked()
    {
        // TODO: Implement ComputePicked
    }

    public void ComputePickingTypeId()
    {
        // TODO: Implement ComputePickingTypeId
    }

    public void ComputeLocationId()
    {
        // TODO: Implement ComputeLocationId
    }

    public void ComputeProductPackagingQty()
    {
        // TODO: Implement ComputeProductPackagingQty
    }

    public void ComputeQuantity()
    {
        // TODO: Implement ComputeQuantity
    }

    public void ComputeQuantityProductUom()
    {
        // TODO: Implement ComputeQuantityProductUom
    }

    public void CheckLotProduct()
    {
        // TODO: Implement CheckLotProduct
    }

    public void CheckPositiveQuantity()
    {
        // TODO: Implement CheckPositiveQuantity
    }

    public void OnchangeProductId()
    {
        // TODO: Implement OnchangeProductId
    }

    public void OnchangeSerialNumber()
    {
        // TODO: Implement OnchangeSerialNumber
    }

    public void OnchangeQuantity()
    {
        // TODO: Implement OnchangeQuantity
    }

    public void OnchangePutawayLocation()
    {
        // TODO: Implement OnchangePutawayLocation
    }

    public void ApplyPutawayStrategy()
    {
        // TODO: Implement ApplyPutawayStrategy
    }

    public Res.Company GetDefaultDestLocation()
    {
        // TODO: Implement GetDefaultDestLocation
    }

    public Dictionary<int, double> GetPutawayAdditionalQty()
    {
        // TODO: Implement GetPutawayAdditionalQty
    }

    public void Init()
    {
        // TODO: Implement Init
    }

    public List<StockMoveLine> Create(List<Dictionary<string, object>> valsList)
    {
        // TODO: Implement Create
    }

    public void Write(Dictionary<string, object> vals)
    {
        // TODO: Implement Write
    }

    public void UnlinkExceptDoneOrCancel()
    {
        // TODO: Implement UnlinkExceptDoneOrCancel
    }

    public void Unlink()
    {
        // TODO: Implement Unlink
    }

    public void ActionDone()
    {
        // TODO: Implement ActionDone
    }

    public Tuple<double, DateTime> SynchronizeQuant(double quantity, Stock.Location location, string action = "available", DateTime inDate = default(DateTime), Dictionary<string, object> quantsValue = null)
    {
        // TODO: Implement SynchronizeQuant
    }

    public List<StockMoveLine> GetSimilarMoveLines()
    {
        // TODO: Implement GetSimilarMoveLines
    }

    public Dictionary<string, object> PrepareNewLotVals()
    {
        // TODO: Implement PrepareNewLotVals
    }

    public void CreateAndAssignProductionLot()
    {
        // TODO: Implement CreateAndAssignProductionLot
    }

    public bool ReservationIsUpdatable(double quantity, Stock.Quant reservedQuant)
    {
        // TODO: Implement ReservationIsUpdatable
    }

    public void LogMessage(Stock.StockPicking record, StockMoveLine move, string template, Dictionary<string, object> vals)
    {
        // TODO: Implement LogMessage
    }

    public void FreeReservation(Product.Product productId, Stock.Location locationId, double quantity, Stock.Lot lotId = null, Stock.QuantPackage packageId = null, Res.Partner ownerId = null, OrderedSet mlIdsToIgnore = null)
    {
        // TODO: Implement FreeReservation
    }

    public Dictionary<string, object> GetAggregatedProperties(StockMoveLine moveLine = null, Stock.StockMove move = null)
    {
        // TODO: Implement GetAggregatedProperties
    }

    public Dictionary<string, object> ComputePackagingQtys(Dictionary<string, object> aggregatedMoveLines)
    {
        // TODO: Implement ComputePackagingQtys
    }

    public Dictionary<string, object> GetAggregatedProductQuantities(Dictionary<string, object> kwargs = null)
    {
        // TODO: Implement GetAggregatedProductQuantities
    }

    public void ComputeSalePrice()
    {
        // TODO: Implement ComputeSalePrice
    }

    public Dictionary<string, object> PrepareStockMoveVals()
    {
        // TODO: Implement PrepareStockMoveVals
    }

    public Dictionary<string, object> CopyQuantInfo(Dictionary<string, object> vals)
    {
        // TODO: Implement CopyQuantInfo
    }

    public Dictionary<string, object> ActionOpenReference()
    {
        // TODO: Implement ActionOpenReference
    }

    public bool ActionPutInPack()
    {
        // TODO: Implement ActionPutInPack
    }

    public Dictionary<string, object> GetRevertInventoryMoveValues()
    {
        // TODO: Implement GetRevertInventoryMoveValues
    }

    public Dictionary<string, object> ActionRevertInventory()
    {
        // TODO: Implement ActionRevertInventory
    }
}
