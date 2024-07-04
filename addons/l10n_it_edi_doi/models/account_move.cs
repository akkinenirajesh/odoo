csharp
public partial class AccountMove
{
    public void ComputeL10nItEdiDoiDate()
    {
        L10nItEdiDoiDate = InvoiceDate ?? Env.Context.Today();
    }

    public void ComputeL10nItEdiDoiUse()
    {
        var saleTypes = Env.Get<AccountMove>().GetSaleTypes();
        L10nItEdiDoiUse = L10nItEdiDoiId != null || (CountryCode == "IT" && saleTypes.Contains(MoveType));
    }

    public void ComputeL10nItEdiDoiId()
    {
        if (!L10nItEdiDoiUse || (State != "draft" && L10nItEdiDoiId == null))
        {
            L10nItEdiDoiId = null;
            return;
        }

        var partner = Partner.CommercialPartnerId;

        var validityWarnings = L10nItEdiDoiId?.GetValidityWarnings(Company, partner, Currency, L10nItEdiDoiDate);
        if (L10nItEdiDoiId != null && (validityWarnings == null || !validityWarnings.Any()))
        {
            return;
        }

        var declaration = Env.Get<L10nItEdiDoi.DeclarationOfIntent>()
            .FetchValidDeclarationOfIntent(Company, partner, Currency, L10nItEdiDoiDate);
        L10nItEdiDoiId = declaration;
    }

    public void ComputeL10nItEdiDoiAmount()
    {
        var tax = Company.L10nItEdiDoiTaxId;
        if (tax == null || L10nItEdiDoiId == null)
        {
            L10nItEdiDoiAmount = 0;
            return;
        }

        var declarationLines = InvoiceLineIds.Where(line => line.TaxIds.Single().Id == tax.Id);
        L10nItEdiDoiAmount = declarationLines.Sum(line => line.PriceTotal) * -DirectionSign;
    }

    public void ComputeL10nItEdiDoiWarning()
    {
        L10nItEdiDoiWarning = "";
        var declaration = L10nItEdiDoiId;

        var showWarning = declaration != null && IsSaleDocument(false) && State != "cancel";
        if (!showWarning)
        {
            return;
        }

        // Implement the rest of the warning computation logic here
        // ...

        L10nItEdiDoiWarning = $"{string.Join("\n", validityWarnings)}\n\n{thresholdWarning}".Trim();
    }

    public void ComputeFiscalPositionId()
    {
        base.ComputeFiscalPositionId();
        var declarationFiscalPosition = Company.L10nItEdiDoiFiscalPositionId;
        if (declarationFiscalPosition != null && L10nItEdiDoiId != null)
        {
            FiscalPositionId = declarationFiscalPosition;
        }
    }

    public override Dictionary<string, object> CopyData(Dictionary<string, object> defaultValues = null)
    {
        var data = base.CopyData(defaultValues);
        var date = Env.Context.Today();
        var validityWarnings = L10nItEdiDoiId?.GetValidityWarnings(Company, CommercialPartnerId, Currency, date, onlyBlocking: true);
        if (validityWarnings != null && validityWarnings.Any())
        {
            data.Remove("L10nItEdiDoiId");
            data.Remove("FiscalPositionId");
        }
        return data;
    }

    public void CheckL10nItEdiDoiId()
    {
        var validityErrors = L10nItEdiDoiId?.GetValidityErrors(Company, Partner.CommercialPartnerId, Currency);
        if (validityErrors != null && validityErrors.Any())
        {
            throw new UserError(string.Join("\n", validityErrors));
        }
    }

    public override void Post(bool soft = true)
    {
        var errors = new List<string>();
        // Implement the posting logic and error checking here
        // ...

        if (errors.Any())
        {
            throw new UserError(string.Join("\n", errors));
        }

        base.Post(soft);
    }

    public void ActionOpenDeclarationOfIntent()
    {
        return new
        {
            name = $"Declaration of Intent for {DisplayName}",
            type = "ir.actions.act_window",
            viewMode = "form",
            resModel = "l10n_it_edi_doi.declaration_of_intent",
            resId = L10nItEdiDoiId.Id
        };
    }
}
