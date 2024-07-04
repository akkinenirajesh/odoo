csharp
public partial class SaleOrderLine
{
    // all the model methods are written here.
    public virtual void ComputeUomQty(double newQty, StockMove stockMove, bool rounding)
    {
        // ...
    }

    public virtual int GetInvoiceLineSequence(int newSequence, int oldSequence)
    {
        // ...
        return newSequence;
    }

    public virtual Dictionary<string, object> PrepareInvoiceLine(Dictionary<string, object> optionalValues)
    {
        // ...
        return new Dictionary<string, object>();
    }

    public virtual void SetAnalyticDistribution(Dictionary<string, object> invLineVals, Dictionary<string, object> optionalValues)
    {
        // ...
    }

    public virtual Dictionary<string, object> PrepareProcurementValues(Guid? groupId)
    {
        // ...
        return new Dictionary<string, object>();
    }

    public virtual void ValidateAnalyticDistribution()
    {
        // ...
    }

    public virtual string GetPartnerDisplay()
    {
        // ...
        return "";
    }

    public virtual bool IsDelivery()
    {
        // ...
        return false;
    }

    public virtual bool IsNotSellableLine()
    {
        // ...
        return false;
    }

    public virtual Dictionary<string, object> GetProductCatalogLinesData(Dictionary<string, object> kwargs)
    {
        // ...
        return new Dictionary<string, object>();
    }

    public virtual double ConvertToSolCurrency(double amount, ResCurrency currency)
    {
        // ...
        return amount;
    }

    public virtual bool HasValuedMoveIds()
    {
        // ...
        return false;
    }

    public virtual DateTime ExpectedDate()
    {
        // ...
        return DateTime.Now;
    }

    public virtual void ActionAddFromCatalog()
    {
        // ...
    }

    public virtual void OnChangeProductIdWarning()
    {
        // ...
    }

    public virtual void OnChangeProductPackagingId()
    {
        // ...
    }

    public virtual void CheckLineUnlink()
    {
        // ...
    }

    public virtual void UnlinkExceptConfirmed()
    {
        // ...
    }
}
