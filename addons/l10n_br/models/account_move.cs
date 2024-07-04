csharp
public partial class AccountMove
{
    public void ComputeL10nLatamDocumentType()
    {
        var brDebitNotes = Env.AccountMove.Search(m => 
            m.State == "draft" && 
            m.CountryCode == "BR" && 
            m.DebitOriginId != null && 
            m.DebitOriginId.L10nLatamDocumentTypeId != null);

        foreach (var move in brDebitNotes)
        {
            move.L10nLatamDocumentTypeId = move.DebitOriginId.L10nLatamDocumentTypeId;
        }

        // Call the base implementation for the remaining moves
        base.ComputeL10nLatamDocumentType();
    }

    public (string WhereString, Dictionary<string, object> Param) GetLastSequenceDomain(bool relaxed = false)
    {
        var (whereString, param) = base.GetLastSequenceDomain(relaxed);

        if (CountryCode == "BR" && L10nLatamUseDocuments)
        {
            whereString += " AND L10nLatamDocumentTypeId = @L10nLatamDocumentTypeId ";
            param["L10nLatamDocumentTypeId"] = L10nLatamDocumentTypeId?.Id ?? 0;
        }

        return (whereString, param);
    }
}
