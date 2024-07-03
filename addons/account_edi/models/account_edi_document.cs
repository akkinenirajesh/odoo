csharp
public partial class AccountEdiDocument
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeEdiContent()
    {
        byte[] res = new byte[0];
        if (State == AccountEdiDocumentState.ToSend || State == AccountEdiDocumentState.ToCancel)
        {
            var move = MoveId;
            var configErrors = EdiFormatId.CheckMoveConfiguration(move);
            if (configErrors.Any())
            {
                res = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", configErrors));
            }
            else
            {
                var moveApplicability = EdiFormatId.GetMoveApplicability(move);
                if (moveApplicability != null && moveApplicability.ContainsKey("EdiContent"))
                {
                    res = moveApplicability["EdiContent"](move);
                }
            }
        }
        EdiContent = Convert.ToBase64String(res);
    }

    public Dictionary<string, object> ActionExportXml()
    {
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_url" },
            { "url", $"/web/content/account.edi.document/{Id}/edi_content" }
        };
    }

    // Other methods would be implemented here...
}
