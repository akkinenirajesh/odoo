csharp
public partial class PaymentProvider
{
    public void ButtonImmediateInstall()
    {
        if (this.Module != null && this.ModuleState != "installed")
        {
            this.Module.ButtonImmediateInstall();
            // Reload the page
        }
    }

    public void ActionToggleIsPublished()
    {
        if (this.State != "disabled")
        {
            this.IsPublished = !this.IsPublished;
        }
        else
        {
            // Raise UserError("You cannot publish a disabled provider.")
        }
    }

    public void ActionViewPaymentMethods()
    {
        // Return a client action to view payment methods
    }

    public void ComputeAvailableCurrencyIds()
    {
        var allCurrencies = Env.Get("res.currency").Search(new Dictionary<string, object>() { { "active_test", false } });
        var supportedCurrencies = GetSupportedCurrencies();
        if (supportedCurrencies.Count < allCurrencies.Count) // Some currencies have been filtered out.
        {
            this.AvailableCurrencies = supportedCurrencies;
        }
        else
        {
            this.AvailableCurrencies = null;
        }
    }

    public void ComputeColor()
    {
        if (this.Module != null && this.ModuleState != "installed")
        {
            this.Color = 4; // blue
        }
        else if (this.State == "disabled")
        {
            this.Color = 3; // yellow
        }
        else if (this.State == "test")
        {
            this.Color = 2; // orange
        }
        else if (this.State == "enabled")
        {
            this.Color = 7; // green
        }
    }

    public void ComputeFeatureSupportFields()
    {
        this.SupportExpressCheckout = null;
        this.SupportManualCapture = null;
        this.SupportRefund = null;
        this.SupportTokenization = null;
    }

    public void OnChangeStateSwitchIsPublished()
    {
        this.IsPublished = this.State == "enabled";
    }

    public void OnChangeStateWarnBeforeDisablingTokens()
    {
        // Display warning
    }

    public void OnChangeCompanyBlockIfExistingTransactions()
    {
        // Raise UserError
    }

    private void ArchiveLinkedTokens()
    {
        // Archive tokens
    }

    private void DeactivateUnsupportedPaymentMethods()
    {
        // Deactivate unsupported payment methods
    }

    private void ActivateDefaultPms()
    {
        // Activate default payment methods
    }

    private void CheckRequiredIfProvider()
    {
        // Check required fields
    }

    private List<Core.Currency> GetSupportedCurrencies()
    {
        // Return supported currencies
        return Env.Get("res.currency").Search(new Dictionary<string, object>() { { "active_test", false } });
    }

    private bool IsTokenizationRequired(Dictionary<string, object> kwargs)
    {
        // Return whether tokenization is required
        return false;
    }

    private bool ShouldBuildInlineForm(bool isValidation)
    {
        // Return whether to build inline form
        return true;
    }

    private double GetValidationAmount()
    {
        // Return validation amount
        return 0.0;
    }

    private Core.Currency GetValidationCurrency()
    {
        // Return validation currency
        return this.Company.Currency;
    }

    private Core.UiView GetRedirectFormView(bool isValidation)
    {
        // Return redirect form view
        return this.RedirectFormView;
    }

    public static void SetupProvider(string providerCode)
    {
        // Perform module-specific setup steps
    }

    public static void RemoveProvider(string providerCode)
    {
        // Remove module-specific data
    }

    private Dictionary<string, object> GetRemovalValues()
    {
        // Return removal values
        return new Dictionary<string, object>() {
            { "Code", "none" },
            { "State", "disabled" },
            { "IsPublished", false },
            { "RedirectFormView", null },
            { "InlineFormView", null },
            { "TokenInlineFormView", null },
            { "ExpressCheckoutFormView", null },
        };
    }

    private string GetProviderName()
    {
        // Return translated name
        return Env.Get("ir.model.fields")._description_selection(this.Code).First(x => x.Value == this.Code).Key;
    }

    private string GetCode()
    {
        return this.Code;
    }

    private HashSet<string> GetDefaultPaymentMethodCodes()
    {
        // Return default payment method codes
        return new HashSet<string>();
    }
}
