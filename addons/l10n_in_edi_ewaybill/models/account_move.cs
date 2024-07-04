csharp
public partial class AccountMove
{
    public void ComputeL10nInEdiEwaybillShowSendButton()
    {
        var ediFormat = Env.Ref<EdiFormat>("l10n_in_edi_ewaybill.edi_in_ewaybill_json_1_03");
        if (ediFormat == null)
        {
            L10nInEdiEwaybillShowSendButton = false;
            return;
        }

        if (IsInvoice() && State == "posted" && CountryCode == "IN")
        {
            var alreadySent = EdiDocumentIds.Any(x => x.EdiFormatId == ediFormat && 
                (x.State == "sent" || x.State == "to_cancel" || x.State == "to_send"));
            L10nInEdiEwaybillShowSendButton = !alreadySent;
        }
        else
        {
            L10nInEdiEwaybillShowSendButton = false;
        }
    }

    public void ComputeL10nInEdiEwaybillDirect()
    {
        var baseType = Env.GetService<IAccountEdiFormat>().L10nInEdiEwaybillBaseIrnOrDirect(this);
        L10nInEdiEwaybillDirectApi = baseType == "direct";
    }

    public Dictionary<string, object> GetL10nInEdiEwaybillResponseJson()
    {
        var l10nInEdi = EdiDocumentIds.FirstOrDefault(i => 
            i.EdiFormatId.Code == "in_ewaybill_1_03" && 
            (i.State == "sent" || i.State == "to_cancel"));

        if (l10nInEdi != null && l10nInEdi.AttachmentId != null)
        {
            var rawData = l10nInEdi.AttachmentId.Raw;
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(rawData);
        }
        return new Dictionary<string, object>();
    }

    public void ButtonCancelPostedMoves()
    {
        var reasonAndRemarksNotSet = new List<AccountMove>();
        foreach (var move in this.Env.Context.GetRecordset<AccountMove>())
        {
            var sendL10nInEdiEwaybill = move.EdiDocumentIds.Any(doc => doc.EdiFormatId.Code == "in_ewaybill_1_03");
            if (sendL10nInEdiEwaybill && (string.IsNullOrEmpty(move.L10nInEdiCancelReason) || string.IsNullOrEmpty(move.L10nInEdiCancelRemarks)))
            {
                reasonAndRemarksNotSet.Add(move);
            }
        }

        if (reasonAndRemarksNotSet.Any())
        {
            throw new UserException($"To cancel E-waybill set cancel reason and remarks at E-waybill tab in: \n{string.Join("\n", reasonAndRemarksNotSet.Select(m => m.Name))}");
        }

        // Call base implementation
        base.ButtonCancelPostedMoves();
    }

    public void L10nInEdiEwaybillSend()
    {
        var ediFormat = Env.Ref<EdiFormat>("l10n_in_edi_ewaybill.edi_in_ewaybill_json_1_03");
        var ediDocumentValsList = new List<Dictionary<string, object>>();

        if (State != "posted")
        {
            throw new UserException("You can only create E-waybill from posted invoice");
        }

        var errors = ediFormat.CheckMoveConfiguration(this);
        if (errors.Any())
        {
            throw new UserException($"Invalid invoice configuration:\n\n{string.Join("\n", errors)}");
        }

        var existingEdiDocument = EdiDocumentIds.FirstOrDefault(x => x.EdiFormatId == ediFormat);
        if (existingEdiDocument != null)
        {
            if (existingEdiDocument.State == "sent" || existingEdiDocument.State == "to_cancel")
            {
                throw new UserException("E-waybill is already created");
            }
            existingEdiDocument.State = "to_send";
            existingEdiDocument.AttachmentId = null;
        }
        else
        {
            ediDocumentValsList.Add(new Dictionary<string, object>
            {
                { "EdiFormatId", ediFormat.Id },
                { "MoveId", Id },
                { "State", "to_send" }
            });
        }

        Env.GetService<IAccountEdiDocument>().Create(ediDocumentValsList);
        Env.Ref<IrCron>("account_edi.ir_cron_edi_network").Trigger();
    }
}
