csharp
public partial class PointOfSale.PosPaymentMethod
{
    public virtual void _ComputeHideUsePaymentTerminal()
    {
        // TODO: Implement logic for _ComputeHideUsePaymentTerminal
    }

    public virtual void _ComputeHideQrCodeMethod()
    {
        // TODO: Implement logic for _ComputeHideQrCodeMethod
    }

    public virtual void _OnchangePaymentMethodType()
    {
        // TODO: Implement logic for _OnchangePaymentMethodType
    }

    public virtual void _OnchangeUsePaymentTerminal()
    {
        // TODO: Implement logic for _OnchangeUsePaymentTerminal
    }

    public virtual void _ComputeOpenSessionIds()
    {
        // TODO: Implement logic for _ComputeOpenSessionIds
    }

    public virtual void _ComputeType()
    {
        // TODO: Implement logic for _ComputeType
    }

    public virtual void _OnchangeJournalId()
    {
        // TODO: Implement logic for _OnchangeJournalId
    }

    public virtual void _ComputeIsCashCount()
    {
        // TODO: Implement logic for _ComputeIsCashCount
    }

    public virtual List<PointOfSale.PosPaymentMethod> Create(List<Dictionary<string, object>> valsList)
    {
        // TODO: Implement logic for Create
        return null;
    }

    public virtual void Write(Dictionary<string, object> vals)
    {
        // TODO: Implement logic for Write
    }

    public virtual void _ForcePaymentMethodTypeValues(Dictionary<string, object> vals, string paymentMethodType, bool ifPresent = false)
    {
        // TODO: Implement logic for _ForcePaymentMethodTypeValues
    }

    public virtual List<Dictionary<string, object>> CopyData(Dictionary<string, object> defaultValues = null)
    {
        // TODO: Implement logic for CopyData
        return null;
    }

    public virtual void _CheckPaymentMethod()
    {
        // TODO: Implement logic for _CheckPaymentMethod
    }

    public virtual void _ComputeQr()
    {
        // TODO: Implement logic for _ComputeQr
    }

    public virtual string GetQrCode(decimal amount, string freeCommunication, string structuredCommunication, int currency, int debtorPartner)
    {
        // TODO: Implement logic for GetQrCode
        return null;
    }

    public virtual List<object> _GetPaymentTerminalSelection()
    {
        // TODO: Implement logic for _GetPaymentTerminalSelection
        return null;
    }

    public virtual List<object> _GetPaymentMethodType()
    {
        // TODO: Implement logic for _GetPaymentMethodType
        return null;
    }

    public virtual List<object> _LoadPosDataDomain(Dictionary<string, object> data)
    {
        // TODO: Implement logic for _LoadPosDataDomain
        return null;
    }

    public virtual List<string> _LoadPosDataFields(int configId)
    {
        // TODO: Implement logic for _LoadPosDataFields
        return null;
    }
}
