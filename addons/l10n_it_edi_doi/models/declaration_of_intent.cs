csharp
public partial class DeclarationOfIntent
{
    public override string ToString()
    {
        return $"{ProtocolNumberPart1}-{ProtocolNumberPart2}";
    }

    public string BuildThresholdWarningMessage(decimal invoiced, decimal notYetInvoiced)
    {
        decimal updatedRemaining = Threshold - invoiced - notYetInvoiced;
        if (updatedRemaining >= 0)
        {
            return string.Empty;
        }

        return string.Format(
            "Pay attention, the threshold of your Declaration of Intent {0} of {1} is exceeded by {2}, this document included.\n" +
            "Invoiced: {3}; Not Yet Invoiced: {4}",
            ToString(),
            Env.FormatCurrency(Threshold, Currency),
            Env.FormatCurrency(-updatedRemaining, Currency),
            Env.FormatCurrency(invoiced, Currency),
            Env.FormatCurrency(notYetInvoiced, Currency)
        );
    }

    public List<string> GetValidityErrors(Core.Company company, Core.Partner partner, Core.Currency currency)
    {
        var errors = new List<string>();

        if (company == null || Company != company)
        {
            errors.Add($"The Declaration of Intent belongs to company {Company.Name}, not {company?.Name}.");
        }

        if (currency == null || Currency != currency)
        {
            errors.Add($"The Declaration of Intent uses currency {Currency.Name}, not {currency?.Name}.");
        }

        if (partner == null || Partner != partner.CommercialPartner)
        {
            errors.Add($"The Declaration of Intent belongs to partner {Partner.Name}, not {partner?.CommercialPartner?.Name}.");
        }

        return errors;
    }

    public List<string> GetValidityWarnings(Core.Company company, Core.Partner partner, Core.Currency currency, DateTime date, decimal invoicedAmount = 0, bool onlyBlocking = false, bool salesOrder = false)
    {
        var errors = GetValidityErrors(company, partner, currency);

        if (State == DeclarationOfIntentState.Draft)
        {
            errors.Add("The Declaration of Intent is in draft.");
        }

        if (invoicedAmount > 0 || !onlyBlocking)
        {
            if (State != DeclarationOfIntentState.Active)
            {
                errors.Add("The Declaration of Intent must be active.");
            }

            if (!salesOrder && (date < StartDate || date > EndDate))
            {
                errors.Add($"The Declaration of Intent is valid from {StartDate:d} to {EndDate:d}, not on {date:d}.");
            }
        }

        return errors;
    }

    public void Validate()
    {
        if (State == DeclarationOfIntentState.Draft)
        {
            State = DeclarationOfIntentState.Active;
        }
    }

    public void ResetToDraft()
    {
        if (State == DeclarationOfIntentState.Active)
        {
            State = DeclarationOfIntentState.Draft;
        }
    }

    public void Reactivate()
    {
        if (State != DeclarationOfIntentState.Active)
        {
            State = DeclarationOfIntentState.Active;
        }
    }

    public void Revoke()
    {
        State = DeclarationOfIntentState.Revoked;
    }

    public void Terminate()
    {
        if (State != DeclarationOfIntentState.Revoked)
        {
            State = DeclarationOfIntentState.Terminated;
        }
    }
}
