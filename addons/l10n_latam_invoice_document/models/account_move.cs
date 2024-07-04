csharp
public partial class AccountMove
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeName()
    {
        if (JournalId.L10nLatamUseDocuments && !L10nLatamDocumentTypeId)
        {
            Name = "/";
        }
        else if (JournalId.L10nLatamUseDocuments && L10nLatamManualDocumentNumber)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Name = "/";
            }
        }
        else
        {
            // Implement the logic for computing name based on the document type
        }
    }

    public void ComputeL10nLatamManualDocumentNumber()
    {
        if (JournalId != null && JournalId.L10nLatamUseDocuments)
        {
            L10nLatamManualDocumentNumber = IsManualDocumentNumber();
        }
        else
        {
            L10nLatamManualDocumentNumber = false;
        }
    }

    private bool IsManualDocumentNumber()
    {
        return JournalId.Type == "purchase";
    }

    public void ComputeL10nLatamDocumentNumber()
    {
        if (!string.IsNullOrEmpty(Name) && Name != "/")
        {
            string name = Name;
            string docCodePrefix = L10nLatamDocumentTypeId?.DocCodePrefix;
            if (!string.IsNullOrEmpty(docCodePrefix) && !string.IsNullOrEmpty(name))
            {
                name = name.Split(' ', 2).Last();
            }
            L10nLatamDocumentNumber = name;
        }
        else
        {
            L10nLatamDocumentNumber = null;
        }
    }

    public void InverseL10nLatamDocumentNumber()
    {
        if (L10nLatamDocumentTypeId != null)
        {
            if (string.IsNullOrEmpty(L10nLatamDocumentNumber))
            {
                Name = "/";
            }
            else
            {
                string formattedNumber = L10nLatamDocumentTypeId.FormatDocumentNumber(L10nLatamDocumentNumber);
                if (L10nLatamDocumentNumber != formattedNumber)
                {
                    L10nLatamDocumentNumber = formattedNumber;
                }
                Name = $"{L10nLatamDocumentTypeId.DocCodePrefix} {formattedNumber}";
            }
        }
    }

    public void OnChangeL10nLatamDocumentTypeId()
    {
        if (JournalId.L10nLatamUseDocuments && L10nLatamDocumentTypeId != null
            && !L10nLatamManualDocumentNumber && State == "draft" && !PostedBefore)
        {
            Name = "/";
            ComputeName();
        }
    }

    public void ComputeL10nLatamAvailableDocumentTypes()
    {
        if (JournalId != null && L10nLatamUseDocuments && CommercialPartnerId != null)
        {
            L10nLatamAvailableDocumentTypeIds = Env.GetAll<Account.L10nLatamDocumentType>()
                .Where(dt => GetL10nLatamDocumentsDomain().All(condition => condition(dt)))
                .ToList();
        }
        else
        {
            L10nLatamAvailableDocumentTypeIds = new List<Account.L10nLatamDocumentType>();
        }
    }

    public void ComputeL10nLatamDocumentType()
    {
        if (State == "draft")
        {
            var documentTypes = L10nLatamAvailableDocumentTypeIds;
            L10nLatamDocumentTypeId = documentTypes.Any() ? documentTypes.First() : null;
        }
    }

    private List<Func<Account.L10nLatamDocumentType, bool>> GetL10nLatamDocumentsDomain()
    {
        List<string> internalTypes = new List<string>();
        if (MoveType == "out_refund" || MoveType == "in_refund")
        {
            internalTypes.Add("credit_note");
        }
        else if (MoveType == "out_invoice" || MoveType == "in_invoice")
        {
            internalTypes.Add("invoice");
            internalTypes.Add("debit_note");
        }
        if (DebitOriginId != null)
        {
            internalTypes.Add("debit_note");
        }
        internalTypes.Add("all");

        return new List<Func<Account.L10nLatamDocumentType, bool>>
        {
            dt => internalTypes.Contains(dt.InternalType),
            dt => dt.CountryId == CompanyId.AccountFiscalCountryId
        };
    }
}
