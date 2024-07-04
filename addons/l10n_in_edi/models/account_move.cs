csharp
public partial class AccountMove
{
    public bool ComputeL10nInEdiShowCancel()
    {
        return Env.EdiDocuments
            .Where(i => i.EdiFormatId.Code == "in_einvoice_1_03" && 
                        (i.State == "sent" || i.State == "to_cancel" || i.State == "cancelled"))
            .Any();
    }

    public void ButtonCancelPostedMoves()
    {
        var reasonAndRemarksNotSet = new List<AccountMove>();

        if (Env.EdiDocuments.Any(doc => doc.EdiFormatId.Code == "in_einvoice_1_03") &&
            (string.IsNullOrEmpty(L10nInEdiCancelReason) || string.IsNullOrEmpty(L10nInEdiCancelRemarks)))
        {
            reasonAndRemarksNotSet.Add(this);
        }

        if (reasonAndRemarksNotSet.Any())
        {
            throw new UserException(
                $"To cancel E-invoice set cancel reason and remarks at Other info tab in invoices: \n{string.Join("\n", reasonAndRemarksNotSet.Select(m => m.Name))}"
            );
        }

        // Call base implementation
        base.ButtonCancelPostedMoves();
    }

    public string GetL10nInEdiResponseJson()
    {
        var l10nInEdi = Env.EdiDocuments
            .FirstOrDefault(i => i.EdiFormatId.Code == "in_einvoice_1_03" && 
                                 (i.State == "sent" || i.State == "to_cancel"));

        if (l10nInEdi != null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<string>(
                System.Text.Encoding.UTF8.GetString(l10nInEdi.Attachment.Raw)
            );
        }
        else
        {
            return "{}";
        }
    }
}
