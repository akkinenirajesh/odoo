csharp
public partial class AccountMove
{
    public void ActionCancelPeppolDocuments()
    {
        if (PeppolMoveState == PeppolMoveState.Processing || PeppolMoveState == PeppolMoveState.Done)
        {
            throw new UserException("Cannot cancel an entry that has already been sent to PEPPOL");
        }
        PeppolMoveState = null;
        SendAndPrintValues = false;
    }

    [Computed]
    public PeppolMoveState ComputePeppolMoveState()
    {
        var canSend = Env.Get<AccountEdiProxyClientUser>().GetCanSendDomain();
        
        if (Company.AccountPeppolProxyState.IsIn(canSend) &&
            CommercialPartner.AccountPeppolIsEndpointValid &&
            State == "posted" &&
            IsSaleDocument(includeReceipts: true) &&
            PeppolMoveState == null)
        {
            return PeppolMoveState.Ready;
        }
        else if (State == "draft" &&
                 IsSaleDocument(includeReceipts: true) &&
                 PeppolMoveState != PeppolMoveState.Processing &&
                 PeppolMoveState != PeppolMoveState.Done)
        {
            return null;
        }
        else
        {
            return PeppolMoveState;
        }
    }

    private bool IsSaleDocument(bool includeReceipts)
    {
        // Implement the logic for IsSaleDocument method here
        throw new NotImplementedException();
    }
}
