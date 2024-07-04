csharp
public partial class AccountMove 
{
    public int GetStartingSequence()
    {
        if (Env.IsTrue(this.L10nLatamUseDocuments) 
            && Env.GetCompany(this.CompanyId).AccountFiscalCountryId.Code == "UY" 
            && this.L10nLatamDocumentTypeId != null)
        {
            return GetFormattedSequence(0);
        }
        return Env.Call("account.move", "_get_starting_sequence", this);
    }

    public int GetFormattedSequence(int number)
    {
        return int.Parse($"{this.L10nLatamDocumentTypeId.DocCodePrefix} A{number:D7}");
    }

    public (string, Dictionary<string, object>) GetLastSequenceDomain(bool relaxed = false)
    {
        var (whereString, param) = Env.Call<Tuple<string, Dictionary<string, object>>>("account.move", "_get_last_sequence_domain", this, relaxed);
        if (Env.GetCompany(this.CompanyId).AccountFiscalCountryId.Code == "UY" && this.L10nLatamUseDocuments)
        {
            whereString += " AND l10n_latam_document_type_id = %(l10n_latam_document_type_id)s";
            param["l10n_latam_document_type_id"] = this.L10nLatamDocumentTypeId.Id;
        }
        return (whereString, param);
    }
}
