csharp
public partial class Onboarding
{
    public void ActionClosePanelAccountInvoice()
    {
        ActionClosePanel("account.onboarding_onboarding_account_invoice");
    }

    public Dictionary<string, object> PrepareRenderingValues()
    {
        var result = base.PrepareRenderingValues();

        var invoiceOnboarding = Env.Ref("account.onboarding_onboarding_account_invoice", false);
        if (this == invoiceOnboarding)
        {
            var step = Env.Ref("account.onboarding_onboarding_step_create_invoice", false);
            if (step != null && step.CurrentStepState == "not_done")
            {
                var hasInvoices = Env.Set<Account.Move>()
                    .Where(m => m.Company == Env.Company && m.MoveType == "out_invoice")
                    .Any();

                if (hasInvoices)
                {
                    step.ActionSetJustDone();
                }
            }
        }

        return result;
    }

    public void ActionClosePanelAccountDashboard()
    {
        ActionClosePanel("account.onboarding_onboarding_account_dashboard");
    }
}
