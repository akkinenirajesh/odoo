csharp
public partial class PaymentResCompany {
    public virtual PaymentResCompany RunPaymentOnboardingStep(int menuId = 0) {
        Env.Company.GetChartOfAccountsOrFail();
        InstallModules(new List<string>() { "payment_stripe" });
        PaymentProvider stripeProvider = Env.PaymentProvider.Search(new List<object>() {
            Env.PaymentProvider.CheckCompanyDomain(Env.Company),
            new Tuple<string, object>("Code", "stripe")
        }, 1);
        if (stripeProvider == null) {
            PaymentProvider baseProvider = Env.Ref("payment.payment_provider_stripe");
            stripeProvider = baseProvider.Copy(new Dictionary<string, object>() {
                {"CompanyId", Env.Company.Id}
            }, true, new Dictionary<string, object>() {
                {"StripeConnectOnboarding", true}
            });
        }
        return stripeProvider.ActionStripeConnectAccount(menuId);
    }

    public virtual void InstallModules(List<string> moduleNames) {
        List<IrModuleModule> modules = Env.IrModuleModule.Search(new List<object>() {
            new Tuple<string, object>("Name", moduleNames)
        });
        modules.Where(m => !new List<string>() { "installed", "to install", "to upgrade" }.Contains(m.State)).ButtonImmediateInstall();
    }
}
