csharp
public partial class AccountMove
{
    public Dictionary<string, object> PrepareEdiTaxDetails(Func<Dictionary<string, object>, bool> filterToApply = null, 
        Func<AccountMoveLine, bool> filterInvlToApply = null, 
        Func<Dictionary<string, object>, Dictionary<string, object>> groupingKeyGenerator = null)
    {
        // Implementation of _prepare_edi_tax_details
        // This method would need to be adapted to work with C# constructs and the new data model
        throw new NotImplementedException();
    }

    public bool IsReadyToBeSent()
    {
        // Implementation of _is_ready_to_be_sent
        var res = base.IsReadyToBeSent();

        if (!res)
            return false;

        var ediDocumentsToSend = EdiDocumentIds.Where(x => x.State == "ToSend");
        return !ediDocumentsToSend.Any();
    }

    public void ButtonCancel()
    {
        // Implementation of button_cancel
        base.ButtonCancel();

        EdiDocumentIds.Where(doc => doc.State != "Sent").ForEach(doc => 
        {
            doc.State = "Cancelled";
            doc.Error = null;
            doc.BlockingLevel = null;
        });

        EdiDocumentIds.Where(doc => doc.State == "Sent").ForEach(doc => 
        {
            doc.State = "ToCancel";
            doc.Error = null;
            doc.BlockingLevel = null;
        });

        EdiDocumentIds.ProcessDocumentsNoWebServices();
        Env.Ref<IrCron>("account_edi.ir_cron_edi_network").Trigger();
    }

    public void ButtonDraft()
    {
        // Implementation of button_draft
        foreach (var move in this)
        {
            if (move.EdiShowCancelButton)
            {
                throw new UserError($"You can't edit the following journal entry {move.DisplayName} because an electronic document has already been sent. Please use the 'Request EDI Cancellation' button instead.");
            }
        }

        base.ButtonDraft();

        EdiDocumentIds.ForEach(doc => 
        {
            doc.Error = null;
            doc.BlockingLevel = null;
        });

        EdiDocumentIds.Where(doc => doc.State == "ToSend").Delete();
    }

    public void ButtonCancelPostedMoves()
    {
        // Implementation of button_cancel_posted_moves
        var toCancelDocuments = new List<AccountEdiDocument>();
        foreach (var move in this)
        {
            move.CheckFiscalyearLockDate();
            var isMoveMoved = false;
            foreach (var doc in move.EdiDocumentIds)
            {
                var moveApplicability = doc.EdiFormatId.GetMoveApplicability(move);
                if (doc.EdiFormatId.NeedsWebServices() &&
                    doc.State == "Sent" &&
                    moveApplicability != null &&
                    moveApplicability.ContainsKey("cancel"))
                {
                    toCancelDocuments.Add(doc);
                    isMoveMoved = true;
                }
            }
            if (isMoveMoved)
            {
                move.MessagePost(body: "A cancellation of the EDI has been requested.");
            }
        }

        toCancelDocuments.ForEach(doc => 
        {
            doc.State = "ToCancel";
            doc.Error = null;
            doc.BlockingLevel = null;
        });
    }

    public void ButtonProcessEdiWebServices()
    {
        ActionProcessEdiWebServices(withCommit: false);
    }

    public void ActionProcessEdiWebServices(bool withCommit = true)
    {
        var docs = EdiDocumentIds.Where(d => (d.State == "ToSend" || d.State == "ToCancel") && d.BlockingLevel != "Error");
        docs.ProcessDocumentsWebServices(withCommit: withCommit);
    }

    public void ActionRetryEdiDocumentsError()
    {
        RetryEdiDocumentsErrorHook();
        EdiDocumentIds.ForEach(doc => 
        {
            doc.Error = null;
            doc.BlockingLevel = null;
        });
        ActionProcessEdiWebServices();
    }

    protected virtual void RetryEdiDocumentsErrorHook()
    {
        // Hook method to be overridden if needed
    }
}
