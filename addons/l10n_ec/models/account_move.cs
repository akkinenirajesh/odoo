csharp
public partial class AccountMove
{
    private static readonly Dictionary<string, List<string>> _documentsMapping = new Dictionary<string, List<string>>
    {
        {"01", new List<string> {"ec_dt_01", "ec_dt_02", "ec_dt_04", "ec_dt_05", "ec_dt_08", "ec_dt_09", "ec_dt_11", "ec_dt_12", "ec_dt_16", "ec_dt_20", "ec_dt_21", "ec_dt_41", "ec_dt_42", "ec_dt_43", "ec_dt_45", "ec_dt_47", "ec_dt_48"}},
        {"02", new List<string> {"ec_dt_03", "ec_dt_04", "ec_dt_05", "ec_dt_09", "ec_dt_19", "ec_dt_41", "ec_dt_294", "ec_dt_344"}},
        // ... Add the rest of the mapping here
    };

    public IEnumerable<LatamDocumentType> GetL10nEcDocumentsAllowed(PartnerIdTypeEc identificationCode)
    {
        var documentsAllowed = new List<LatamDocumentType>();
        if (_documentsMapping.TryGetValue(identificationCode.ToString(), out var documentRefs))
        {
            foreach (var documentRef in documentRefs)
            {
                var documentAllowed = Env.Ref<LatamDocumentType>($"l10n_ec.{documentRef}");
                if (documentAllowed != null)
                {
                    documentsAllowed.Add(documentAllowed);
                }
            }
        }
        return documentsAllowed;
    }

    public IEnumerable<LatamDocumentType> GetL10nLatamDocumentsDomain()
    {
        var domain = base.GetL10nLatamDocumentsDomain();
        if (this.Country.Code == "EC" && this.Journal.L10nLatamUseDocuments)
        {
            if (this.DebitOriginId != null)
            {
                domain = domain.Where(d => d.InternalType == "debit_note");
            }
            else if (this.MoveType == "out_invoice" || this.MoveType == "in_invoice")
            {
                domain = domain.Where(d => d.InternalType == "invoice");
            }
            var allowedDocuments = GetL10nEcDocumentsAllowed(PartnerIdTypeEc.GetAtsCodeForPartner(this.Partner, this.MoveType));
            domain = domain.Where(d => allowedDocuments.Contains(d));
        }
        return domain;
    }

    public string GetEcFormattedSequence(int number = 0)
    {
        return $"{this.L10nLatamDocumentTypeId.DocCodePrefix} {this.Journal.L10nEcEntity}-{this.Journal.L10nEcEmission}-{number:D9}";
    }

    public override string GetStartingSequence()
    {
        if (this.Journal.L10nLatamUseDocuments && this.Company.Country.Code == "EC")
        {
            if (this.L10nLatamDocumentTypeId != null)
            {
                return GetEcFormattedSequence();
            }
        }
        return base.GetStartingSequence();
    }

    public override (string WhereString, Dictionary<string, object> Param) GetLastSequenceDomain(bool relaxed = false)
    {
        var (whereString, param) = base.GetLastSequenceDomain(relaxed);
        if (this.Country.Code == "EC" && this.L10nLatamUseDocuments)
        {
            var internalType = this.L10nLatamDocumentTypeId.InternalType;
            var documentTypes = Env.Search<LatamDocumentType>(d => 
                d.InternalType == internalType && d.Country.Code == "EC");
            
            if (documentTypes.Any())
            {
                whereString += " AND l10n_latam_document_type_id IN @l10n_latam_document_type_id";
                param["l10n_latam_document_type_id"] = documentTypes.Select(d => d.Id).ToList();
            }
        }
        return (whereString, param);
    }
}
