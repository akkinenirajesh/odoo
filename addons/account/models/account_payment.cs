csharp
public partial class AccountPayment
{
    public override string ToString()
    {
        return MoveId?.Name ?? "Draft Payment";
    }

    public void MarkAsSent()
    {
        IsMoveSent = true;
    }

    public void UnmarkAsSent()
    {
        IsMoveSent = false;
    }

    public void ActionPost()
    {
        if (RequirePartnerBankAccount && !PartnerBankId.AllowOutPayment)
        {
            throw new UserException($"To record payments with {PaymentMethodLineId.Name}, the recipient bank account must be manually validated. You should go on the partner bank account of {PartnerId.DisplayName} in order to validate it.");
        }

        MoveId.Post(false);

        if (IsInternalTransfer && PairedInternalTransferPaymentId == null)
        {
            CreatePairedInternalTransferPayment();
        }
    }

    public void ActionCancel()
    {
        MoveId.ButtonCancel();
    }

    public void ButtonRequestCancel()
    {
        MoveId.ButtonRequestCancel();
    }

    public void ActionDraft()
    {
        MoveId.ButtonDraft();
    }

    public ActionResult ButtonOpenInvoices()
    {
        // Implementation for opening invoices
        // This would return an appropriate ActionResult object
        throw new NotImplementedException();
    }

    public ActionResult ButtonOpenBills()
    {
        // Implementation for opening bills
        // This would return an appropriate ActionResult object
        throw new NotImplementedException();
    }

    public ActionResult ButtonOpenStatementLines()
    {
        // Implementation for opening statement lines
        // This would return an appropriate ActionResult object
        throw new NotImplementedException();
    }

    public ActionResult ButtonOpenJournalEntry()
    {
        // Implementation for opening journal entry
        // This would return an appropriate ActionResult object
        throw new NotImplementedException();
    }

    public ActionResult ActionOpenDestinationJournal()
    {
        // Implementation for opening destination journal
        // This would return an appropriate ActionResult object
        throw new NotImplementedException();
    }

    private void CreatePairedInternalTransferPayment()
    {
        // Implementation for creating paired internal transfer payment
        throw new NotImplementedException();
    }
}
