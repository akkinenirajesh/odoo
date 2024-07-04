csharp
public partial class OnboardingStep {
    public object ActionValidateStepPaymentProvider() {
        return this.Env.Call("action_validate_step", "payment.onboarding_onboarding_step_payment_provider");
    }
}
