csharp
public partial class OnboardingStep
{
    public string ActionOpenStepPaymentProvider()
    {
        Env.Company.PaymentOnboardingPaymentMethod = "stripe";
        var menu = Env.Ref("account_payment.payment_provider_menu", raiseIfNotFound: false);
        int? menuId = menu?.Id;
        return Env.Company.RunPaymentOnboardingStep(menuId);
    }

    public object ActionValidateStepPaymentProvider()
    {
        var validationResponse = base.ActionValidateStepPaymentProvider();
        ActionValidateStep("account_payment.onboarding_onboarding_step_payment_provider");
        return validationResponse;
    }
}
