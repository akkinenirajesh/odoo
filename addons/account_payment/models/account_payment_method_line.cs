csharp
public partial class AccountPaymentMethodLine
{
    public string ComputeName()
    {
        // Implement the base ComputeName logic here
        // ...

        if (PaymentProviderId != null && string.IsNullOrEmpty(Name))
        {
            Name = PaymentProviderId.Name;
        }

        return Name;
    }

    public Payment.Provider ComputePaymentProviderId()
    {
        var journal = JournalId;
        var company = journal?.CompanyId;

        if (company != null && PaymentMethodId != null && PaymentProviderId == null)
        {
            // Implement the logic to get journals payment method information
            // and compute the PaymentProviderId
            // ...
        }

        return PaymentProviderId;
    }

    public void OnDeleteValidation()
    {
        var activeProvider = PaymentProviderId?.Where(provider => 
            provider.State == Payment.ProviderState.Enabled || 
            provider.State == Payment.ProviderState.Test).ToList();

        if (activeProvider != null && activeProvider.Any())
        {
            throw new UserException(
                $"You can't delete a payment method that is linked to a provider in the enabled " +
                $"or test state.\nLinked providers(s): {string.Join(", ", activeProvider.Select(a => a.DisplayName))}"
            );
        }
    }

    public ActionResult ActionOpenProviderForm()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Provider",
            ViewMode = "form",
            ResModel = "Payment.Provider",
            Target = "current",
            ResId = PaymentProviderId?.Id ?? 0
        };
    }
}
